---
name: aws-cdk-expert
description: Expert guidance for infrastructure as code using AWS CDK with C#, including stack composition, constructs, best practices, testing, CI/CD integration, and production-ready patterns
---

# AWS CDK Expert (C#)

## When to use this skill

Use this skill when:
- Writing AWS CDK infrastructure code in C#
- Designing reusable CDK constructs
- Implementing multi-stack applications
- Setting up CI/CD for CDK deployments
- Testing CDK infrastructure
- Migrating from CloudFormation to CDK
- Implementing best practices for IaC

## CDK Project Structure

### Recommended Layout
```
MyApp/
├── src/
│   ├── MyApp.Infrastructure/
│   │   ├── Stacks/
│   │   │   ├── NetworkStack.cs
│   │   │   ├── DatabaseStack.cs
│   │   │   ├── ComputeStack.cs
│   │   │   └── MonitoringStack.cs
│   │   ├── Constructs/
│   │   │   ├── SecureApiConstruct.cs
│   │   │   ├── ObservableServiceConstruct.cs
│   │   │   └── DatabaseClusterConstruct.cs
│   │   ├── Config/
│   │   │   ├── EnvironmentConfig.cs
│   │   │   └── AppSettings.cs
│   │   └── Program.cs
│   └── MyApp.Infrastructure.Tests/
│       ├── Stacks/
│       └── Constructs/
└── cdk.json
```

## Basic CDK Application Setup

### Program.cs
```csharp
using Amazon.CDK;
using MyApp.Infrastructure.Stacks;
using MyApp.Infrastructure.Config;

namespace MyApp.Infrastructure;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var app = new App();
        
        // Load configuration
        var config = EnvironmentConfig.Load(app.Node);
        
        // Create stacks with cross-stack references
        var networkStack = new NetworkStack(app, "NetworkStack", new StackProps
        {
            Env = config.Environment,
            Description = "Network infrastructure including VPC, subnets, and security groups",
            Tags = config.CommonTags
        }, config);
        
        var databaseStack = new DatabaseStack(app, "DatabaseStack", new StackProps
        {
            Env = config.Environment,
            Description = "RDS Aurora cluster and related resources",
            Tags = config.CommonTags
        }, config, networkStack);
        
        var computeStack = new ComputeStack(app, "ComputeStack", new StackProps
        {
            Env = config.Environment,
            Description = "ECS cluster, services, and load balancer",
            Tags = config.CommonTags
        }, config, networkStack, databaseStack);
        
        var monitoringStack = new MonitoringStack(app, "MonitoringStack", new StackProps
        {
            Env = config.Environment,
            Description = "CloudWatch dashboards and alarms",
            Tags = config.CommonTags
        }, config, computeStack);
        
        // Add dependencies
        databaseStack.AddDependency(networkStack);
        computeStack.AddDependency(networkStack);
        computeStack.AddDependency(databaseStack);
        monitoringStack.AddDependency(computeStack);
        
        app.Synth();
    }
}
```

### Environment Configuration
```csharp
public class EnvironmentConfig
{
    public required Amazon.CDK.Environment Environment { get; init; }
    public required string EnvironmentName { get; init; }
    public required Dictionary<string, string> CommonTags { get; init; }
    public required NetworkConfig Network { get; init; }
    public required DatabaseConfig Database { get; init; }
    public required ComputeConfig Compute { get; init; }
    
    public static EnvironmentConfig Load(IConstruct scope)
    {
        var environmentName = scope.Node.TryGetContext("environment")?.ToString() ?? "dev";
        
        return environmentName.ToLower() switch
        {
            "prod" => CreateProductionConfig(),
            "staging" => CreateStagingConfig(),
            "dev" => CreateDevelopmentConfig(),
            _ => throw new ArgumentException($"Unknown environment: {environmentName}")
        };
    }
    
    private static EnvironmentConfig CreateProductionConfig()
    {
        return new EnvironmentConfig
        {
            Environment = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = "us-east-1"
            },
            EnvironmentName = "prod",
            CommonTags = new Dictionary<string, string>
            {
                ["Environment"] = "production",
                ["ManagedBy"] = "CDK",
                ["Application"] = "MyApp"
            },
            Network = new NetworkConfig
            {
                VpcCidr = "10.0.0.0/16",
                MaxAzs = 3,
                NatGateways = 3
            },
            Database = new DatabaseConfig
            {
                InstanceType = "r6g.2xlarge",
                MinCapacity = 2,
                MaxCapacity = 8,
                BackupRetentionDays = 30
            },
            Compute = new ComputeConfig
            {
                DesiredCount = 6,
                MinCapacity = 4,
                MaxCapacity = 20,
                Cpu = 2048,
                MemoryMiB = 4096
            }
        };
    }
    
    private static EnvironmentConfig CreateDevelopmentConfig()
    {
        return new EnvironmentConfig
        {
            Environment = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = "us-east-1"
            },
            EnvironmentName = "dev",
            CommonTags = new Dictionary<string, string>
            {
                ["Environment"] = "development",
                ["ManagedBy"] = "CDK",
                ["Application"] = "MyApp"
            },
            Network = new NetworkConfig
            {
                VpcCidr = "10.1.0.0/16",
                MaxAzs = 2,
                NatGateways = 1
            },
            Database = new DatabaseConfig
            {
                InstanceType = "t4g.medium",
                MinCapacity = 1,
                MaxCapacity = 2,
                BackupRetentionDays = 7
            },
            Compute = new ComputeConfig
            {
                DesiredCount = 2,
                MinCapacity = 1,
                MaxCapacity = 4,
                Cpu = 512,
                MemoryMiB = 1024
            }
        };
    }
}

public record NetworkConfig
{
    public required string VpcCidr { get; init; }
    public required int MaxAzs { get; init; }
    public required int NatGateways { get; init; }
}

public record DatabaseConfig
{
    public required string InstanceType { get; init; }
    public required int MinCapacity { get; init; }
    public required int MaxCapacity { get; init; }
    public required int BackupRetentionDays { get; init; }
}

public record ComputeConfig
{
    public required int DesiredCount { get; init; }
    public required int MinCapacity { get; init; }
    public required int MaxCapacity { get; init; }
    public required int Cpu { get; init; }
    public required int MemoryMiB { get; init; }
}
```

## Stack Examples

### Network Stack
```csharp
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Constructs;

namespace MyApp.Infrastructure.Stacks;

public class NetworkStack : Stack
{
    public IVpc Vpc { get; }
    public ISecurityGroup ApplicationSecurityGroup { get; }
    public ISecurityGroup DatabaseSecurityGroup { get; }
    public ISecurityGroup LoadBalancerSecurityGroup { get; }
    
    public NetworkStack(
        Construct scope,
        string id,
        IStackProps props,
        EnvironmentConfig config) : base(scope, id, props)
    {
        // Create VPC with public and private subnets
        Vpc = new Vpc(this, "VPC", new VpcProps
        {
            MaxAzs = config.Network.MaxAzs,
            IpAddresses = IpAddresses.Cidr(config.Network.VpcCidr),
            NatGateways = config.Network.NatGateways,
            SubnetConfiguration = new[]
            {
                new SubnetConfiguration
                {
                    Name = "Public",
                    SubnetType = SubnetType.PUBLIC,
                    CidrMask = 24
                },
                new SubnetConfiguration
                {
                    Name = "Private",
                    SubnetType = SubnetType.PRIVATE_WITH_EGRESS,
                    CidrMask = 20
                },
                new SubnetConfiguration
                {
                    Name = "Database",
                    SubnetType = SubnetType.PRIVATE_ISOLATED,
                    CidrMask = 24
                }
            },
            EnableDnsHostnames = true,
            EnableDnsSupport = true
        });
        
        // Add VPC Flow Logs
        Vpc.AddFlowLog("FlowLog", new FlowLogOptions
        {
            Destination = FlowLogDestination.ToCloudWatchLogs()
        });
        
        // Security Groups
        LoadBalancerSecurityGroup = new SecurityGroup(this, "LoadBalancerSG", new SecurityGroupProps
        {
            Vpc = Vpc,
            Description = "Security group for Application Load Balancer",
            AllowAllOutbound = true
        });
        
        LoadBalancerSecurityGroup.AddIngressRule(
            Peer.AnyIpv4(),
            Port.Tcp(443),
            "Allow HTTPS from internet");
        
        ApplicationSecurityGroup = new SecurityGroup(this, "ApplicationSG", new SecurityGroupProps
        {
            Vpc = Vpc,
            Description = "Security group for ECS tasks",
            AllowAllOutbound = true
        });
        
        ApplicationSecurityGroup.AddIngressRule(
            LoadBalancerSecurityGroup,
            Port.Tcp(8080),
            "Allow traffic from Load Balancer");
        
        DatabaseSecurityGroup = new SecurityGroup(this, "DatabaseSG", new SecurityGroupProps
        {
            Vpc = Vpc,
            Description = "Security group for RDS database",
            AllowAllOutbound = false
        });
        
        DatabaseSecurityGroup.AddIngressRule(
            ApplicationSecurityGroup,
            Port.Tcp(5432),
            "Allow PostgreSQL from application");
        
        // VPC Endpoints for AWS services
        Vpc.AddGatewayEndpoint("S3Endpoint", new GatewayVpcEndpointOptions
        {
            Service = GatewayVpcEndpointAwsService.S3
        });
        
        Vpc.AddInterfaceEndpoint("EcrEndpoint", new InterfaceVpcEndpointOptions
        {
            Service = InterfaceVpcEndpointAwsService.ECR
        });
        
        Vpc.AddInterfaceEndpoint("EcrDockerEndpoint", new InterfaceVpcEndpointOptions
        {
            Service = InterfaceVpcEndpointAwsService.ECR_DOCKER
        });
        
        Vpc.AddInterfaceEndpoint("SecretsManagerEndpoint", new InterfaceVpcEndpointOptions
        {
            Service = InterfaceVpcEndpointAwsService.SECRETS_MANAGER
        });
        
        // Outputs
        _ = new CfnOutput(this, "VpcId", new CfnOutputProps
        {
            Value = Vpc.VpcId,
            Description = "VPC ID",
            ExportName = $"{config.EnvironmentName}-vpc-id"
        });
    }
}
```

### Database Stack with Aurora
```csharp
using Amazon.CDK;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.SecretsManager;
using Constructs;

namespace MyApp.Infrastructure.Stacks;

public class DatabaseStack : Stack
{
    public IDatabaseCluster Cluster { get; }
    public ISecret DatabaseSecret { get; }
    
    public DatabaseStack(
        Construct scope,
        string id,
        IStackProps props,
        EnvironmentConfig config,
        NetworkStack networkStack) : base(scope, id, props)
    {
        // Create secret for database credentials
        DatabaseSecret = new Secret(this, "DatabaseSecret", new SecretProps
        {
            SecretName = $"{config.EnvironmentName}/database/credentials",
            GenerateSecretString = new SecretStringGenerator
            {
                SecretStringTemplate = "{\"username\":\"dbadmin\"}",
                GenerateStringKey = "password",
                ExcludeCharacters = "/@\" '\\",
                PasswordLength = 32
            }
        });
        
        // Create Aurora PostgreSQL cluster
        Cluster = new DatabaseCluster(this, "DatabaseCluster", new DatabaseClusterProps
        {
            Engine = DatabaseClusterEngine.AuroraPostgres(new AuroraPostgresClusterEngineProps
            {
                Version = AuroraPostgresEngineVersion.VER_15_3
            }),
            Credentials = Credentials.FromSecret(DatabaseSecret),
            DefaultDatabaseName = "myapp",
            InstanceProps = new Amazon.CDK.AWS.RDS.InstanceProps
            {
                InstanceType = InstanceType.Of(
                    InstanceClass.BURSTABLE4_GRAVITON,
                    InstanceSize.MEDIUM),
                Vpc = networkStack.Vpc,
                VpcSubnets = new SubnetSelection
                {
                    SubnetType = SubnetType.PRIVATE_ISOLATED
                },
                SecurityGroups = new[] { networkStack.DatabaseSecurityGroup }
            },
            Instances = config.Database.MinCapacity,
            Backup = new BackupProps
            {
                Retention = Duration.Days(config.Database.BackupRetentionDays),
                PreferredWindow = "03:00-04:00"
            },
            PreferredMaintenanceWindow = "sun:04:00-sun:05:00",
            CloudwatchLogsExports = new[] { "postgresql" },
            CloudwatchLogsRetention = Amazon.CDK.AWS.Logs.RetentionDays.ONE_MONTH,
            StorageEncrypted = true,
            DeletionProtection = config.EnvironmentName == "prod",
            RemovalPolicy = config.EnvironmentName == "prod" 
                ? RemovalPolicy.RETAIN 
                : RemovalPolicy.DESTROY
        });
        
        // Add auto scaling for Aurora Serverless v2
        var scalableTarget = Cluster.AddRotationSingleUser();
        
        // CloudWatch alarms
        var cpuAlarm = Cluster.MetricCPUUtilization()
            .CreateAlarm(this, "DatabaseCPUAlarm", new CreateAlarmOptions
            {
                Threshold = 80,
                EvaluationPeriods = 2,
                AlarmDescription = "Database CPU utilization is too high"
            });
        
        var connectionsAlarm = Cluster.MetricDatabaseConnections()
            .CreateAlarm(this, "DatabaseConnectionsAlarm", new CreateAlarmOptions
            {
                Threshold = 100,
                EvaluationPeriods = 2,
                AlarmDescription = "Too many database connections"
            });
        
        // Outputs
        _ = new CfnOutput(this, "DatabaseEndpoint", new CfnOutputProps
        {
            Value = Cluster.ClusterEndpoint.Hostname,
            Description = "Database cluster endpoint",
            ExportName = $"{config.EnvironmentName}-db-endpoint"
        });
        
        _ = new CfnOutput(this, "DatabaseSecretArn", new CfnOutputProps
        {
            Value = DatabaseSecret.SecretArn,
            Description = "Database credentials secret ARN",
            ExportName = $"{config.EnvironmentName}-db-secret-arn"
        });
    }
}
```

### ECS Fargate Stack
```csharp
using Amazon.CDK;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.ECR;
using Constructs;

namespace MyApp.Infrastructure.Stacks;

public class ComputeStack : Stack
{
    public IApplicationLoadBalancedFargateService Service { get; }
    
    public ComputeStack(
        Construct scope,
        string id,
        IStackProps props,
        EnvironmentConfig config,
        NetworkStack networkStack,
        DatabaseStack databaseStack) : base(scope, id, props)
    {
        // Create ECS cluster
        var cluster = new Cluster(this, "Cluster", new ClusterProps
        {
            Vpc = networkStack.Vpc,
            ClusterName = $"{config.EnvironmentName}-cluster",
            ContainerInsights = true
        });
        
        // Reference existing ECR repository
        var repository = Repository.FromRepositoryName(
            this, 
            "Repository", 
            "myapp-api");
        
        // Create Fargate service with ALB
        Service = new ApplicationLoadBalancedFargateService(
            this,
            "FargateService",
            new ApplicationLoadBalancedFargateServiceProps
            {
                Cluster = cluster,
                ServiceName = $"{config.EnvironmentName}-api-service",
                DesiredCount = config.Compute.DesiredCount,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromEcrRepository(repository, "latest"),
                    ContainerName = "api",
                    ContainerPort = 8080,
                    Environment = new Dictionary<string, string>
                    {
                        ["ASPNETCORE_ENVIRONMENT"] = config.EnvironmentName,
                        ["ASPNETCORE_URLS"] = "http://+:8080"
                    },
                    Secrets = new Dictionary<string, Secret>
                    {
                        ["ConnectionStrings__Database"] = Secret.FromSecretsManager(
                            databaseStack.DatabaseSecret,
                            "connectionString")
                    },
                    LogDriver = LogDriver.AwsLogs(new AwsLogDriverProps
                    {
                        StreamPrefix = "api",
                        LogRetention = RetentionDays.ONE_MONTH
                    })
                },
                Cpu = config.Compute.Cpu,
                MemoryLimitMiB = config.Compute.MemoryMiB,
                PublicLoadBalancer = true,
                SecurityGroups = new[] { networkStack.ApplicationSecurityGroup },
                TaskSubnets = new SubnetSelection
                {
                    SubnetType = SubnetType.PRIVATE_WITH_EGRESS
                },
                Certificate = // Add ACM certificate for HTTPS
                    Amazon.CDK.AWS.CertificateManager.Certificate.FromCertificateArn(
                        this,
                        "Certificate",
                        "arn:aws:acm:us-east-1:123456789012:certificate/xxx"),
                RedirectHttp = true,
                HealthCheckGracePeriod = Duration.Seconds(60)
            });
        
        // Configure health check
        Service.TargetGroup.ConfigureHealthCheck(new Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck
        {
            Path = "/health",
            Interval = Duration.Seconds(30),
            Timeout = Duration.Seconds(5),
            HealthyThresholdCount = 2,
            UnhealthyThresholdCount = 3,
            HealthyHttpCodes = "200"
        });
        
        // Grant database access
        databaseStack.DatabaseSecret.GrantRead(Service.TaskDefinition.TaskRole);
        databaseStack.Cluster.Connections.AllowDefaultPortFrom(
            Service.Service.Connections,
            "Allow from ECS tasks");
        
        // Configure auto scaling
        var scaling = Service.Service.AutoScaleTaskCount(new EnableScalingProps
        {
            MinCapacity = config.Compute.MinCapacity,
            MaxCapacity = config.Compute.MaxCapacity
        });
        
        scaling.ScaleOnCpuUtilization("CpuScaling", new CpuUtilizationScalingProps
        {
            TargetUtilizationPercent = 70,
            ScaleInCooldown = Duration.Seconds(60),
            ScaleOutCooldown = Duration.Seconds(60)
        });
        
        scaling.ScaleOnRequestCount("RequestScaling", new RequestCountScalingProps
        {
            RequestsPerTarget = 1000,
            TargetGroup = Service.TargetGroup
        });
        
        // Add CloudWatch alarms
        Service.Service.MetricCpuUtilization()
            .CreateAlarm(this, "ServiceCpuAlarm", new CreateAlarmOptions
            {
                Threshold = 80,
                EvaluationPeriods = 2,
                AlarmDescription = "Service CPU is too high"
            });
        
        Service.TargetGroup.MetricTargetResponseTime()
            .CreateAlarm(this, "ResponseTimeAlarm", new CreateAlarmOptions
            {
                Threshold = 1000,
                EvaluationPeriods = 2,
                AlarmDescription = "Response time is too high"
            });
        
        // Outputs
        _ = new CfnOutput(this, "LoadBalancerUrl", new CfnOutputProps
        {
            Value = Service.LoadBalancer.LoadBalancerDnsName,
            Description = "Load Balancer URL",
            ExportName = $"{config.EnvironmentName}-alb-url"
        });
    }
}
```

## Custom Constructs

### Reusable API Service Construct
```csharp
using Amazon.CDK;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Constructs;

namespace MyApp.Infrastructure.Constructs;

public interface IObservableServiceProps
{
    ICluster Cluster { get; }
    string ServiceName { get; }
    IContainerImage Image { get; }
    int ContainerPort { get; }
    Dictionary<string, string> Environment { get; }
    Dictionary<string, Secret> Secrets { get; }
    IVpc Vpc { get; }
    int Cpu { get; }
    int MemoryMiB { get; }
    int DesiredCount { get; }
}

public class ObservableServiceConstruct : Construct
{
    public ApplicationLoadBalancedFargateService Service { get; }
    
    public ObservableServiceConstruct(
        Construct scope,
        string id,
        IObservableServiceProps props) : base(scope, id)
    {
        Service = new ApplicationLoadBalancedFargateService(
            this,
            "Service",
            new ApplicationLoadBalancedFargateServiceProps
            {
                Cluster = props.Cluster,
                ServiceName = props.ServiceName,
                DesiredCount = props.DesiredCount,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = props.Image,
                    ContainerPort = props.ContainerPort,
                    Environment = props.Environment,
                    Secrets = props.Secrets,
                    LogDriver = LogDriver.AwsLogs(new AwsLogDriverProps
                    {
                        StreamPrefix = props.ServiceName,
                        LogRetention = Amazon.CDK.AWS.Logs.RetentionDays.ONE_MONTH
                    }),
                    EnableLogging = true
                },
                Cpu = props.Cpu,
                MemoryLimitMiB = props.MemoryMiB,
                PublicLoadBalancer = true
            });
        
        // Add X-Ray daemon sidecar
        Service.TaskDefinition.AddContainer("xray-daemon", new ContainerDefinitionOptions
        {
            Image = ContainerImage.FromRegistry("amazon/aws-xray-daemon:latest"),
            Cpu = 32,
            MemoryReservationMiB = 256,
            PortMappings = new[]
            {
                new PortMapping { ContainerPort = 2000, Protocol = Protocol.UDP }
            },
            Logging = LogDriver.AwsLogs(new AwsLogDriverProps
            {
                StreamPrefix = $"{props.ServiceName}-xray",
                LogRetention = Amazon.CDK.AWS.Logs.RetentionDays.ONE_WEEK
            })
        });
        
        // Grant X-Ray permissions
        Service.TaskDefinition.TaskRole.AddManagedPolicy(
            Amazon.CDK.AWS.IAM.ManagedPolicy.FromAwsManagedPolicyName("AWSXRayDaemonWriteAccess"));
        
        // Add standard alarms
        AddStandardAlarms(props.ServiceName);
    }
    
    private void AddStandardAlarms(string serviceName)
    {
        Service.Service.MetricCpuUtilization()
            .CreateAlarm(this, "CpuAlarm", new CreateAlarmOptions
            {
                Threshold = 80,
                EvaluationPeriods = 2,
                AlarmDescription = $"{serviceName} CPU is high",
                ActionsEnabled = true
            });
        
        Service.TargetGroup.MetricUnhealthyHostCount()
            .CreateAlarm(this, "UnhealthyHostsAlarm", new CreateAlarmOptions
            {
                Threshold = 1,
                EvaluationPeriods = 1,
                AlarmDescription = $"{serviceName} has unhealthy hosts"
            });
        
        Service.TargetGroup.MetricTargetResponseTime()
            .CreateAlarm(this, "ResponseTimeAlarm", new CreateAlarmOptions
            {
                Threshold = 1000,
                EvaluationPeriods = 3,
                AlarmDescription = $"{serviceName} response time is high"
            });
    }
}
```

## Testing CDK Stacks

### Stack Tests
```csharp
using Amazon.CDK;
using Amazon.CDK.Assertions;
using MyApp.Infrastructure.Stacks;
using Xunit;

namespace MyApp.Infrastructure.Tests.Stacks;

public class NetworkStackTests
{
    [Fact]
    public void VpcCreatedWithCorrectConfiguration()
    {
        // Arrange
        var app = new App();
        var config = CreateTestConfig();
        
        // Act
        var stack = new NetworkStack(app, "TestStack", new StackProps(), config);
        var template = Template.FromStack(stack);
        
        // Assert
        template.ResourceCountIs("AWS::EC2::VPC", 1);
        template.HasResourceProperties("AWS::EC2::VPC", new Dictionary<string, object>
        {
            ["EnableDnsHostnames"] = true,
            ["EnableDnsSupport"] = true
        });
    }
    
    [Fact]
    public void SecurityGroupsCreatedWithCorrectRules()
    {
        // Arrange
        var app = new App();
        var config = CreateTestConfig();
        var stack = new NetworkStack(app, "TestStack", new StackProps(), config);
        var template = Template.FromStack(stack);
        
        // Assert
        template.ResourceCountIs("AWS::EC2::SecurityGroup", 3);
        
        // Verify ALB security group allows HTTPS
        template.HasResourceProperties("AWS::EC2::SecurityGroupIngress", new Dictionary<string, object>
        {
            ["IpProtocol"] = "tcp",
            ["FromPort"] = 443,
            ["ToPort"] = 443
        });
    }
    
    [Fact]
    public void VpcEndpointsCreated()
    {
        // Arrange
        var app = new App();
        var config = CreateTestConfig();
        var stack = new NetworkStack(app, "TestStack", new StackProps(), config);
        var template = Template.FromStack(stack);
        
        // Assert - Gateway endpoints
        template.ResourceCountIs("AWS::EC2::VPCEndpoint", Match.AtLeast(1));
    }
    
    [Theory]
    [InlineData("prod", 3)]
    [InlineData("dev", 1)]
    public void NatGatewayCountMatchesEnvironment(string environment, int expectedCount)
    {
        // Arrange
        var app = new App();
        app.Node.SetContext("environment", environment);
        var config = EnvironmentConfig.Load(app.Node);
        
        // Act
        var stack = new NetworkStack(app, "TestStack", new StackProps(), config);
        var template = Template.FromStack(stack);
        
        // Assert
        template.ResourceCountIs("AWS::EC2::NatGateway", expectedCount);
    }
    
    private static EnvironmentConfig CreateTestConfig()
    {
        return new EnvironmentConfig
        {
            Environment = new Amazon.CDK.Environment
            {
                Account = "123456789012",
                Region = "us-east-1"
            },
            EnvironmentName = "test",
            CommonTags = new Dictionary<string, string>(),
            Network = new NetworkConfig
            {
                VpcCidr = "10.0.0.0/16",
                MaxAzs = 2,
                NatGateways = 1
            },
            Database = new DatabaseConfig
            {
                InstanceType = "t3.micro",
                MinCapacity = 1,
                MaxCapacity = 2,
                BackupRetentionDays = 7
            },
            Compute = new ComputeConfig
            {
                DesiredCount = 2,
                MinCapacity = 1,
                MaxCapacity = 4,
                Cpu = 256,
                MemoryMiB = 512
            }
        };
    }
}
```

### Snapshot Tests
```csharp
[Fact]
public void DatabaseStackMatchesSnapshot()
{
    // Arrange
    var app = new App();
    var config = CreateTestConfig();
    var networkStack = new NetworkStack(app, "NetworkStack", new StackProps(), config);
    
    // Act
    var stack = new DatabaseStack(app, "DatabaseStack", new StackProps(), config, networkStack);
    var template = Template.FromStack(stack);
    
    // Assert
    var json = template.ToJSON();
    Snapshot.Match(json); // Uses Verify or similar library
}
```

## CDK Pipelines

### Self-Mutating Pipeline
```csharp
using Amazon.CDK;
using Amazon.CDK.Pipelines;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Constructs;

namespace MyApp.Infrastructure.Stacks;

public class PipelineStack : Stack
{
    public PipelineStack(Construct scope, string id, IStackProps props) : base(scope, id, props)
    {
        // Source from GitHub
        var sourceOutput = new Artifact_();
        var sourceAction = new GitHubSourceAction(new GitHubSourceActionProps
        {
            ActionName = "GitHub",
            Owner = "myorg",
            Repo = "myapp",
            OauthToken = SecretValue.SecretsManager("github-token"),
            Output = sourceOutput,
            Branch = "main"
        });
        
        // Create CDK pipeline
        var pipeline = new CodePipeline(this, "Pipeline", new CodePipelineProps
        {
            PipelineName = "MyAppPipeline",
            Synth = new ShellStep("Synth", new ShellStepProps
            {
                Input = CodePipelineSource.GitHub("myorg/myapp", "main", new GitHubSourceOptions
                {
                    Authentication = SecretValue.SecretsManager("github-token")
                }),
                Commands = new[]
                {
                    "cd src/MyApp.Infrastructure",
                    "npm install -g aws-cdk",
                    "dotnet restore",
                    "dotnet build",
                    "dotnet test",
                    "cdk synth"
                },
                PrimaryOutputDirectory = "src/MyApp.Infrastructure/cdk.out"
            }),
            SelfMutation = true,
            CrossAccountKeys = true,
            DockerEnabledForSynth = true
        });
        
        // Add staging deployment
        var stagingStage = new ApplicationStage(this, "Staging", new StageProps
        {
            Env = new Amazon.CDK.Environment
            {
                Account = "111111111111",
                Region = "us-east-1"
            }
        }, "staging");
        
        pipeline.AddStage(stagingStage, new AddStageOpts
        {
            Pre = new[]
            {
                new ShellStep("ValidateConfig", new ShellStepProps
                {
                    Commands = new[] { "echo 'Validating staging configuration'" }
                })
            },
            Post = new[]
            {
                new ShellStep("IntegrationTests", new ShellStepProps
                {
                    Commands = new[]
                    {
                        "dotnet test src/MyApp.IntegrationTests",
                        "echo 'Running smoke tests'"
                    },
                    EnvFromCfnOutputs = new Dictionary<string, CfnOutput>
                    {
                        ["API_URL"] = stagingStage.ApiUrl
                    }
                })
            }
        });
        
        // Add production deployment with manual approval
        var prodStage = new ApplicationStage(this, "Production", new StageProps
        {
            Env = new Amazon.CDK.Environment
            {
                Account = "222222222222",
                Region = "us-east-1"
            }
        }, "prod");
        
        pipeline.AddStage(prodStage, new AddStageOpts
        {
            Pre = new[]
            {
                new ManualApprovalStep("PromoteToProduction")
            }
        });
    }
}

public class ApplicationStage : Stage
{
    public CfnOutput ApiUrl { get; }
    
    public ApplicationStage(
        Construct scope,
        string id,
        IStageProps props,
        string environmentName) : base(scope, id, props)
    {
        var app = new App();
        var config = EnvironmentConfig.Load(app.Node);
        
        var networkStack = new NetworkStack(this, "NetworkStack", new StackProps
        {
            Env = props.Env
        }, config);
        
        var databaseStack = new DatabaseStack(this, "DatabaseStack", new StackProps
        {
            Env = props.Env
        }, config, networkStack);
        
        var computeStack = new ComputeStack(this, "ComputeStack", new StackProps
        {
            Env = props.Env
        }, config, networkStack, databaseStack);
        
        ApiUrl = new CfnOutput(computeStack, "ApiUrl", new CfnOutputProps
        {
            Value = computeStack.Service.LoadBalancer.LoadBalancerDnsName
        });
    }
}
```

## Best Practices

### 1. Use Constructs for Reusability
```csharp
// Bad: Repeating code
var service1 = new ApplicationLoadBalancedFargateService(...);
// Add alarms
// Add X-Ray
// Configure auto-scaling

var service2 = new ApplicationLoadBalancedFargateService(...);
// Repeat same configuration

// Good: Use a construct
var service1 = new ObservableServiceConstruct(this, "Service1", props);
var service2 = new ObservableServiceConstruct(this, "Service2", props);
```

### 2. Environment-Specific Configuration
```csharp
// Load from context
var config = EnvironmentConfig.Load(app.Node);

// Deploy: cdk deploy --context environment=prod
```

### 3. Use Aspects for Cross-Cutting Concerns
```csharp
public class TagAspect : IAspect
{
    private readonly Dictionary<string, string> _tags;
    
    public TagAspect(Dictionary<string, string> tags)
    {
        _tags = tags;
    }
    
    public void Visit(IConstruct node)
    {
        if (node is CfnResource resource)
        {
            foreach (var (key, value) in _tags)
            {
                Tags.Of(resource).Add(key, value);
            }
        }
    }
}

// Usage
Aspects.Of(app).Add(new TagAspect(new Dictionary<string, string>
{
    ["CostCenter"] = "Engineering",
    ["Project"] = "MyApp"
}));
```

### 4. Resource Removal Policies
```csharp
// Production: Retain critical resources
new DatabaseCluster(this, "Database", new DatabaseClusterProps
{
    // ...
    RemovalPolicy = config.EnvironmentName == "prod" 
        ? RemovalPolicy.RETAIN 
        : RemovalPolicy.DESTROY,
    DeletionProtection = config.EnvironmentName == "prod"
});
```

## Common CDK Commands

```bash
# Initialize new project
cdk init app --language csharp

# List stacks
cdk list

# Synthesize CloudFormation template
cdk synth

# Show differences
cdk diff

# Deploy all stacks
cdk deploy --all

# Deploy specific stack
cdk deploy NetworkStack

# Deploy with context
cdk deploy --context environment=prod --all

# Destroy stacks
cdk destroy --all

# Watch mode (hot reload)
cdk watch

# Bootstrap CDK in account/region
cdk bootstrap aws://123456789012/us-east-1
```

## Resources

- [AWS CDK Documentation](https://docs.aws.amazon.com/cdk/)
- [CDK API Reference (.NET)](https://docs.aws.amazon.com/cdk/api/v2/dotnet/api/)
- [CDK Patterns](https://cdkpatterns.com/)
- [AWS CDK Examples](https://github.com/aws-samples/aws-cdk-examples)
