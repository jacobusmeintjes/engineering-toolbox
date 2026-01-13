using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using TodoCli.Commands;
using TodoCli.Infrastructure.Configuration;
using TodoCli.Infrastructure.Storage;
using TodoCli.Output;
using TodoCli.Services;

namespace TodoCli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // Make service provider accessible to commands
        ServiceProviderAccessor.ServiceProvider = serviceProvider;

        // Create root command
        var rootCommand = new RootCommand("TODO CLI - A cross-platform task manager");

        // Get services for commands
        var taskService = serviceProvider.GetRequiredService<ITaskService>();
        var taskFilter = serviceProvider.GetRequiredService<TaskFilter>();
        var tableFormatter = serviceProvider.GetRequiredService<TableFormatter>();
        var consoleWriter = serviceProvider.GetRequiredService<IConsoleWriter>();
        var colorProvider = serviceProvider.GetRequiredService<ColorProvider>();

        // Register commands
        rootCommand.AddCommand(new AddCommand());
        rootCommand.AddCommand(new ListCommand(taskService, taskFilter, tableFormatter, consoleWriter));
        rootCommand.AddCommand(new CompleteCommand(taskService, consoleWriter));
        rootCommand.AddCommand(new ShowCommand(taskService, consoleWriter, colorProvider));
        rootCommand.AddCommand(new UpdateCommand(taskService, consoleWriter, colorProvider));
        rootCommand.AddCommand(new DeleteCommand(taskService, consoleWriter, colorProvider));

        // Execute
        return await rootCommand.InvokeAsync(args);
    }

    static void ConfigureServices(IServiceCollection services)
    {
        // Infrastructure
        services.AddSingleton<IFileStorage, FileStorage>();
        services.AddSingleton<StoragePathProvider>();

        // Repository
        services.AddSingleton<ITaskRepository, JsonTaskRepository>();

        // Services
        services.AddSingleton<ITaskService, TaskService>();
        services.AddSingleton<TaskFilter>();

        // Output
        services.AddSingleton<ColorProvider>();
        services.AddSingleton<TableFormatter>();
        services.AddSingleton<IConsoleWriter, ConsoleWriter>();
    }
}
