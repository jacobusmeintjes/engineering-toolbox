using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace sonarqube_codecharta.AppHost.Extensions
{
    public static class SonarQubeExtensions
    {
        public static IResourceBuilder<ContainerResource> AddSonarQube(
            this IDistributedApplicationBuilder builder,
            string username, string currentPassword,
            string newPassword)
        {
            var sonarQube = builder.AddContainer("sonar-qube", "sonarqube", "latest")
                .WithEnvironment("SONAR_ES_BOOTSTRAP_CHECKS_DISABLE", "true")
                .WithHttpEndpoint(targetPort: 9000)
                .WithHttpHealthCheck("/api/server/version", 200)
                .WithLifetime(ContainerLifetime.Persistent);

            //builder.Eventing.Subscribe<BeforeResourceStartedEvent>(sonarQube.Resource, async (@event, sc) => { });

            builder.Eventing.Subscribe<ResourceReadyEvent>(sonarQube.Resource, async (@event, sc) =>
            {
                @event.Resource.TryGetAnnotationsOfType(out IEnumerable<EndpointAnnotation>? endpoints);

                if (endpoints is null) return;

                var endpoint = endpoints.FirstOrDefault();

                if (endpoint is null) return;

                var endpointUri = endpoint.AllocatedEndpoint?.UriString;

                if (string.IsNullOrEmpty(endpointUri)) return;

                Environment.SetEnvironmentVariable("SONAR_QUBE_URL", endpointUri);

                var code = await UpdateAdminPasswordAsync(username, currentPassword, newPassword);

                @event.Resource.Annotations.Add(new SonarQubeTokenResourceAnnotation(code));
            });


            return sonarQube;
        }


        private static async Task<string> UpdateAdminPasswordAsync(string username, string currentPassword, string newPassword)
        {
            var sonarQubeUrl = Environment.GetEnvironmentVariable("SONAR_QUBE_URL");

            if (string.IsNullOrEmpty(sonarQubeUrl)) return string.Empty;
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = false
                });
            var page = await browser.NewPageAsync();

            await page.GotoAsync(sonarQubeUrl);
            await page.GetByLabel("Login").FillAsync(username);
            await page.GetByLabel("Password").FillAsync(currentPassword);

            var loginButton = page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { NameString = "Log in", });
            await loginButton.ClickAsync();
            try
            {

                await page.WaitForURLAsync("**/reset_password", new PageWaitForURLOptions { Timeout = 5_000 });
                if (page.Url.EndsWith("/account/reset_password"))
                {
                    await page.Locator("#old_password").FillAsync(currentPassword);
                    await page.Locator("#create-password").FillAsync(newPassword);
                    await page.Locator("#confirm-password").FillAsync(newPassword);
                    await page.Locator("#change-password").ClickAsync();
                }

            }
            catch
            {
                await page.GetByLabel("Password").FillAsync(newPassword);

                loginButton = page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { NameString = "Log in", });
                await loginButton.ClickAsync();
            }
            try
            {
                var laterButton = page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { NameString = "Later", });
                await laterButton.ClickAsync(new LocatorClickOptions { Timeout = 1_000 });
            }
            catch (Exception ex)
            {
            }

            try
            {
                var response = await page.GotoAsync($"{sonarQubeUrl}/account/security");
                try
                {
                    var laterButton = page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { NameString = "Later" });
                    await laterButton.ClickAsync(new LocatorClickOptions { Timeout = 1_000 });
                }
                catch
                {
                }

                await page.Locator("#token-name").FillAsync(Guid.NewGuid().ToString());
                await page.Locator("#react-select-2-placeholder").ClickAsync(new LocatorClickOptions { Timeout = 1_000 });
                var selectOption = page.Locator("#react-select-2-option-2");
                await selectOption.ClickAsync(new LocatorClickOptions { Timeout = 1_000 });
                //await page.EvalOnSelectorAsync<string>("#react-select-2-placeholder", "sel => sel.options[0].textContent");
                var generateButton = page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { NameString = "Generate" });
                await generateButton.ClickAsync(new LocatorClickOptions { Timeout = 1_000 });

                var code = await page.Locator("code").InnerTextAsync();

                if (!string.IsNullOrEmpty(code))
                {
                    Environment.SetEnvironmentVariable("SONAR_QUBE_TOKEN", code);
                    return code;
                }
            }
            catch
            {
            }

            return string.Empty;
        }

        public static IResourceBuilder<ProjectResource> WithCodeChartaAnalyzer(this IResourceBuilder<ProjectResource> builder,
            IResourceBuilder<ContainerResource> codeChartAnalyzer)
        {
            if (codeChartAnalyzer is null) throw new ArgumentNullException(nameof(codeChartAnalyzer));

            builder.WaitFor(codeChartAnalyzer);
            builder.WithCommand(name: "run-codecharta",
               displayName: "Run Code Charta",
               executeCommand: context => ExecuteCodeChartaAnalyzer(builder, context, codeChartAnalyzer),
               updateState: context => onCodeChartaState(builder, context));
            return builder;
        }

        private static async Task<ExecuteCommandResult> ExecuteCodeChartaAnalyzer(IResourceBuilder<ProjectResource> builder,
            ExecuteCommandContext context, IResourceBuilder<ContainerResource> codeChartAnalyzer)
        {
            var token = Environment.GetEnvironmentVariable("SONAR_QUBE_TOKEN");
            var endpointUri = Environment.GetEnvironmentVariable("SONAR_QUBE_URL")?.Replace("localhost", "host.docker.internal");

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(endpointUri))
            {
                await RunTool("docker", new List<string>
                {
                    "ps -a -f \"name=test\" -q"
                    //$"exec -it $(docker ps -a -f \"name=test\" -q) ccsh sonarimport -u=squ_418c025d1ddb0c2cc3fe8855813666d5cdbc11c5 --output-file=/data/output/webfrontend.sonar --merge-modules {endpointUri} webfrontend"
                    //"exec", "-it", $"$(docker ps -a -f \"name={codeChartAnalyzer.Resource.Name}\" -q)", $"ccsh sonarimport -u={token} --output-file=/data/output/{builder.Resource.Name}.sonar --merge-modules {endpointUri} {builder.Resource.Name}"
                }, "C:\\Program Files\\Docker\\Docker\\resources\\bin");

            }

            return new ExecuteCommandResult { Success = true };
        }

        private static ResourceCommandState onCodeChartaState(IResourceBuilder<ProjectResource> builder,
            UpdateCommandStateContext context)
        {
            var token = Environment.GetEnvironmentVariable("SONAR_QUBE_TOKEN");

            if (context.ResourceSnapshot.HealthStatus == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy
                || string.IsNullOrEmpty(token))
            {
                return ResourceCommandState.Disabled;
            }

            return ResourceCommandState.Enabled;
        }

        public static IResourceBuilder<ProjectResource> WithSonarQubeAnalyzer(
            this IResourceBuilder<ProjectResource> builder,
            IResourceBuilder<ContainerResource> sonarQube,
            bool useProjectPathOnly)
        {
            if (sonarQube is null) throw new ArgumentNullException(nameof(sonarQube));

            builder.WaitFor(sonarQube);
            builder.WithCommand(name: "run-sonarqube",
                displayName: "Run Sonar Qube",
                executeCommand: context => ExecuteSonarQube(builder, context, builder.Resource.Name, useProjectPathOnly),
                updateState: context => onStateCheck(builder, context));

            return builder;
        }

        private static ResourceCommandState onStateCheck(IResourceBuilder<ProjectResource> builder,
            UpdateCommandStateContext context)
        {
            var token = Environment.GetEnvironmentVariable("SONAR_QUBE_TOKEN");

            if (context.ResourceSnapshot.HealthStatus == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy
                || string.IsNullOrEmpty(token))
            {
                return ResourceCommandState.Disabled;
            }

            return ResourceCommandState.Enabled;
        }

        private static async Task<ExecuteCommandResult> ExecuteSonarQube(IResourceBuilder<ProjectResource> builder,
            ExecuteCommandContext context, string projectKey, bool useProjectPathOnly)
        {
            var projectPath = ((dynamic)builder.Resource.Annotations[0]).ProjectPath;

            var projectFile = new FileInfo(projectPath);
            var projectDirectory = projectFile.Directory;

            if (!useProjectPathOnly)
            {
                var solutionFile = string.Empty;

                if (projectDirectory == null)
                {
                    return new ExecuteCommandResult { Success = false, ErrorMessage = "Project Directory does not exist." };
                }

                while (string.IsNullOrEmpty(solutionFile))
                {
                    solutionFile = projectDirectory.GetFiles("*.sln").FirstOrDefault()?.Name;

                    if (string.IsNullOrEmpty(solutionFile))
                    {
                        if (projectDirectory.Parent == null)
                        {
                            return new ExecuteCommandResult { Success = false, ErrorMessage = "Solution file could not be found." };
                        }

                        projectDirectory = projectDirectory.Parent;
                    }
                }
            }


            var projectDirectoryPath = projectDirectory?.FullName;

            var token = Environment.GetEnvironmentVariable("SONAR_QUBE_TOKEN");
            var endpointUri = Environment.GetEnvironmentVariable("SONAR_QUBE_URL");

            if (!string.IsNullOrEmpty(token))
            {
                if (!string.IsNullOrEmpty(endpointUri))
                {
                    await RunTool("dotnet", $"sonarscanner begin /k:\"{projectKey}\" /d:sonar.token=\"{token}\" /d:sonar.host.url=\"{endpointUri}\"", projectDirectoryPath);
                    await RunTool("dotnet", $"build {projectFile} --no-incremental", projectDirectoryPath);

                    if (!useProjectPathOnly)
                    {
                        await RunTool("dotnet-coverage", $"collect \"dotnet test\" -f xml -o \"{projectKey}-coverage.xml\"", projectDirectoryPath);
                    }

                    await RunTool("dotnet", $"sonarscanner end /d:sonar.token=\"{token}\"", projectDirectoryPath);
                }
                else
                {
                    return new ExecuteCommandResult { Success = false, ErrorMessage = "SonarQube endpoint is not set." };
                }
            }
            else
            {
                return new ExecuteCommandResult { Success = false, ErrorMessage = "SonarQube Token is not set." };
            }



            return new ExecuteCommandResult { Success = true };
        }

        static async Task RunTool(string commandName, IEnumerable<string> commandArguments, string workingDirectory)
        {
            var processStartInfo = new ProcessStartInfo(commandName, commandArguments)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using var process = new Process() { EnableRaisingEvents = true };
            process.StartInfo = processStartInfo;

            
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                processStartInfo.WorkingDirectory = workingDirectory;
            }

            process.OutputDataReceived += (s, ev) =>
            {
                if (string.IsNullOrEmpty(ev.Data)) return;

                Console.WriteLine(ev.Data);
            };

            process.ErrorDataReceived += (s, ev) =>
            {
                if (string.IsNullOrEmpty(ev.Data)) return;

                Console.WriteLine(ev.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();


            await process.WaitForExitAsync();

            processStartInfo = null;
        }

        static async Task RunTool(string commandName, string commandArguments, string workingDirectory = "")
        {
            var processStartInfo = new ProcessStartInfo(commandName, commandArguments)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                processStartInfo.WorkingDirectory = workingDirectory;
            }

            using var process = new Process() { EnableRaisingEvents = true };
            process.StartInfo = processStartInfo;

            process.OutputDataReceived += (s, ev) =>
            {
                if (string.IsNullOrEmpty(ev.Data)) return;

                Console.WriteLine(ev.Data);
            };

            process.ErrorDataReceived += (s, ev) =>
            {
                if (string.IsNullOrEmpty(ev.Data)) return;

                Console.WriteLine(ev.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();


            await process.WaitForExitAsync();

            processStartInfo = null;
        }

        public static string GetSonarQubeToken(this IResourceBuilder<ContainerResource> builder)
        {
            if (builder.Resource.TryGetAnnotationsOfType<SonarQubeTokenResourceAnnotation>(out var tokens))
            {
                var token = tokens?.FirstOrDefault()?.Token;
            }
            return string.Empty;
        }
    }

    public class SonarQubeTokenResourceAnnotation(string token) : IResourceAnnotation
    {
        public string Token => token;
    }



}