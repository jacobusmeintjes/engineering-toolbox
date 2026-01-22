---
name: aws-solutions-architect
description: Expert guidance for designing AWS cloud architectures including compute, storage, networking, security, high availability, disaster recovery, cost optimization, and best practices for enterprise workloads
---

# AWS Solutions Architect Expert

## When to use this skill

Use this skill when:
- Designing AWS cloud architectures
- Selecting appropriate AWS services
- Implementing high availability and disaster recovery
- Optimizing costs and performance
- Designing secure, compliant architectures
- Planning cloud migrations
- Troubleshooting AWS infrastructure issues
- Implementing CI/CD on AWS

## Core Architecture Principles

### Well-Architected Framework

1. **Operational Excellence**: Run and monitor systems
2. **Security**: Protect information and systems
3. **Reliability**: Recover from failures, scale dynamically
4. **Performance Efficiency**: Use resources efficiently
5. **Cost Optimization**: Avoid unnecessary costs
6. **Sustainability**: Minimize environmental impacts

## Compute Services Selection

### When to use EC2
```
✅ Good for:
- Custom OS requirements
- GPU workloads (ML/graphics)
- Legacy applications needing full control
- Persistent workloads with consistent usage
- Windows workloads

Instance types for .NET:
- c6i/c7i: Compute optimized (.NET API services)
- r6i/r7i: Memory optimized (cache-heavy apps)
- t3/t4g: Burstable (dev/test)
```

### When to use ECS/Fargate
```
✅ Good for:
- Containerized .NET applications
- Microservices architectures
- No server management needed
- Variable workloads
- Integration with AWS services

Example: ASP.NET Core API on Fargate
- Auto-scaling based on requests
- No infrastructure management
- Pay only for used resources
```

### When to use Lambda
```
✅ Good for:
- Event-driven architectures
- Serverless .NET APIs (minimal APIs)
- Background processing
- Scheduled tasks
- Pay-per-execution model

⚠️ Limitations:
- 15-minute max execution
- Cold starts (use provisioned concurrency)
- .NET 8 supported (Native AOT for best performance)
```

### When to use App Runner
```
✅ Good for:
- Simple containerized .NET web apps
- Source-to-production in minutes
- Automatic scaling
- Lower complexity than ECS

Perfect for: Internal tools, admin panels, simple APIs
```

## Database Services Selection

### RDS (Relational Database Service)
```
SQL Server:
- Best for: Existing SQL Server workloads
- Supports: Express, Web, Standard, Enterprise
- Use Multi-AZ for high availability
- Read replicas for read-heavy workloads

PostgreSQL:
- Best for: Modern .NET apps with EF Core
- Excellent JSON support
- Aurora PostgreSQL for high performance
- pg_vector for AI/ML embeddings

MySQL/MariaDB:
- Cost-effective
- Wide compatibility
- Aurora MySQL for better performance
```

### DynamoDB
```
✅ Good for:
- High-scale NoSQL workloads
- Session storage
- Gaming leaderboards
- IoT data streams
- Sub-10ms latency requirements

Key patterns:
- Single-table design
- GSI for alternative access patterns
- DynamoDB Streams for change data capture
```

### ElastiCache
```
Redis:
- Session storage
- Rate limiting
- Real-time analytics
- Pub/sub messaging
- Orleans GrainDirectory backing

Memcached:
- Simple caching
- Horizontally scalable
- No persistence needed
```

### DocumentDB
```
MongoDB-compatible
- Best for: MongoDB migrations
- Fully managed
- Automatic backups
```

## Storage Architecture

### S3 (Simple Storage Service)
```
Storage Classes:
- Standard: Frequent access (<$0.023/GB)
- Intelligent-Tiering: Unknown/changing patterns
- Standard-IA: Infrequent access
- Glacier Instant: Archive with instant retrieval
- Glacier Deep Archive: Long-term archive

Best practices:
- Use S3 lifecycle policies
- Enable versioning for critical data
- Use S3 Transfer Acceleration for global uploads
- Implement S3 presigned URLs for secure uploads
```

### EFS (Elastic File System)
```
✅ Good for:
- Shared file storage across EC2/ECS
- Content management systems
- Development environments
- Legacy applications needing NFS

Performance modes:
- General Purpose: Most workloads
- Max I/O: Highly parallel workloads
```

### EBS (Elastic Block Store)
```
Volume types:
- gp3: General purpose SSD (most workloads)
- io2: High-performance SSD (databases)
- st1: Throughput optimized HDD (big data)
- sc1: Cold HDD (infrequent access)

Always:
- Enable EBS encryption
- Take regular snapshots
- Use gp3 over gp2 (better price/performance)
```

## Networking Architecture

### VPC Design Best Practices
```
Standard VPC layout:
- CIDR: /16 (e.g., 10.0.0.0/16)
- Public subnets: /24 per AZ (10.0.1.0/24, 10.0.2.0/24)
- Private subnets: /20 per AZ (10.0.16.0/20, 10.0.32.0/20)
- Database subnets: /24 per AZ (10.0.11.0/24, 10.0.12.0/24)

Multi-AZ deployment:
- Minimum 2 AZs (3 for critical workloads)
- Spread across multiple availability zones
- Use NAT Gateway per AZ for HA
```

### Load Balancing
```
Application Load Balancer (ALB):
✅ Use for:
- HTTP/HTTPS traffic
- Path-based routing (/api/* → API service)
- Host-based routing (api.example.com → API)
- WebSocket support
- Target: ECS tasks, EC2, Lambda

Network Load Balancer (NLB):
✅ Use for:
- TCP/UDP traffic
- Ultra-low latency
- Static IP requirements
- PrivateLink endpoints
- Millions of requests per second

Features:
- Health checks
- SSL termination
- Sticky sessions
- WAF integration (ALB only)
```

### API Gateway
```
Types:
1. REST API:
   - Full API management
   - Request/response transformation
   - API keys and usage plans
   - Caching

2. HTTP API:
   - Lower cost
   - Better performance
   - Simpler (70% cheaper than REST)
   - JWT authorization

3. WebSocket API:
   - Real-time bi-directional communication
   - Chat applications
   - Live dashboards

Integration patterns:
- Lambda proxy integration
- HTTP proxy to private ALB
- VPC Link for private resources
- Mock integrations for testing
```

### CloudFront (CDN)
```
Use cases:
- Static website hosting
- API acceleration
- Video streaming
- Dynamic content caching

Best practices:
- Use Origin Shield for additional caching
- Configure appropriate cache behaviors
- Implement signed URLs/cookies for private content
- Use Lambda@Edge for edge computing
```

## Security Architecture

### Identity and Access Management (IAM)
```
Best practices:
1. Never use root account (enable MFA)
2. Use IAM roles, not access keys
3. Principle of least privilege
4. Use IAM Policy Conditions
5. Regularly rotate credentials

Service roles for .NET apps:
- EC2 Instance Profiles
- ECS Task Roles
- Lambda Execution Roles
```

Example IAM Policy:
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:GetObject",
        "s3:PutObject"
      ],
      "Resource": "arn:aws:s3:::my-bucket/*",
      "Condition": {
        "StringEquals": {
          "s3:x-amz-server-side-encryption": "AES256"
        }
      }
    }
  ]
}
```

### Security Groups vs NACLs
```
Security Groups (Stateful):
- Instance/ENI level
- Only allow rules
- Evaluated as whole
✅ Primary security mechanism

Network ACLs (Stateless):
- Subnet level
- Allow and deny rules
- Evaluated in order
✅ Secondary defense layer

Typical pattern:
1. NACL: Broad subnet-level controls
2. SG: Specific instance-level controls
```

### Secrets Management
```
AWS Secrets Manager:
✅ Use for:
- Database credentials
- API keys
- OAuth tokens
- Automatic rotation

AWS Systems Manager Parameter Store:
✅ Use for:
- Application configuration
- Simple strings
- Cost-effective (free tier)
- No automatic rotation

.NET integration:
builder.Configuration.AddSecretsManager(
    region: RegionEndpoint.USEast1,
    configurator: options => {
        options.SecretFilter = secret => secret.Name.StartsWith("MyApp/");
    });
```

### Encryption
```
At Rest:
- S3: SSE-S3, SSE-KMS, SSE-C
- EBS: Enable encryption by default
- RDS: Enable encryption at creation
- DynamoDB: Encryption enabled

In Transit:
- ALB: SSL/TLS termination
- CloudFront: HTTPS only
- VPN: AWS Site-to-Site VPN
- API Gateway: TLS 1.2+

AWS KMS:
- Customer Master Keys (CMK)
- Automatic key rotation
- Audit with CloudTrail
- Envelope encryption for large data
```

## High Availability Patterns

### Multi-AZ Deployment
```
Compute:
- Deploy across multiple AZs
- Use Auto Scaling Groups
- Minimum 2 instances per AZ

Database:
- RDS Multi-AZ (synchronous replication)
- Aurora: 6 copies across 3 AZs
- DynamoDB: Automatic multi-AZ

Storage:
- S3: Automatically replicated across AZs
- EBS: Take snapshots, stored in S3
- EFS: Automatically replicated
```

### Disaster Recovery Strategies
```
1. Backup & Restore (RPO: hours, RTO: hours)
   - Lowest cost
   - Periodic backups to S3
   - Restore when needed

2. Pilot Light (RPO: minutes, RTO: hours)
   - Core services running
   - Scaled up during disaster
   - Moderate cost

3. Warm Standby (RPO: seconds, RTO: minutes)
   - Scaled-down replica running
   - Scale up during disaster
   - Higher cost

4. Multi-Region Active-Active (RPO: real-time, RTO: seconds)
   - Full capacity in multiple regions
   - Route53 health checks
   - Highest cost

For banking/financial:
- Usually Warm Standby or Active-Active
- RDS Cross-Region Read Replicas
- S3 Cross-Region Replication
- DynamoDB Global Tables
```

### Auto Scaling
```
Target tracking:
- CPU utilization: 70%
- Request count per target: 1000
- Custom CloudWatch metrics

Step scaling:
- Add capacity in steps
- More aggressive based on alarm breach

Scheduled scaling:
- Predictable load patterns
- Business hours scaling

Application Auto Scaling:
- ECS services
- DynamoDB tables
- Aurora replicas
```

## Monitoring and Observability

### CloudWatch
```
Metrics:
- EC2: CPU, Network, Disk
- RDS: Connections, CPU, IOPS
- Lambda: Invocations, Duration, Errors
- Custom: Application metrics

Logs:
- Centralized log aggregation
- Log Groups and retention
- Insights for querying
- Lambda/ECS automatic integration

Alarms:
- SNS notifications
- Auto Scaling triggers
- EventBridge integration
```

### AWS X-Ray
```
Distributed tracing:
- Track requests across services
- Identify performance bottlenecks
- Service maps
- Trace analysis

.NET integration:
services.AddAWSXRayRecorder();
app.UseXRay("MyApp");
```

### Container Insights
```
ECS/EKS monitoring:
- Container-level metrics
- Performance dashboards
- Log aggregation
- CloudWatch embedded metrics
```

## Cost Optimization

### Compute Savings
```
1. Reserved Instances (1-3 years)
   - Up to 72% savings
   - Standard: Region/AZ locked
   - Convertible: Can change instance type

2. Savings Plans
   - Up to 72% savings
   - More flexible than RIs
   - Compute or EC2 Savings Plans

3. Spot Instances
   - Up to 90% savings
   - For fault-tolerant workloads
   - Batch processing, data analysis

4. Right-sizing
   - Use CloudWatch metrics
   - Downsize over-provisioned instances
   - Use AWS Compute Optimizer
```

### Storage Optimization
```
S3:
- Lifecycle policies to Glacier
- Intelligent-Tiering for unknown patterns
- Delete incomplete multipart uploads
- Use S3 Storage Lens

EBS:
- Delete unattached volumes
- Use gp3 instead of gp2
- Take snapshots, delete old volumes

RDS:
- Stop dev/test instances when not in use
- Use Aurora Serverless v2 for variable workloads
```

### Monitoring Costs
```
Tools:
- Cost Explorer: Historical analysis
- Budgets: Alert on overspending
- Cost Anomaly Detection: ML-based alerts
- Compute Optimizer: Rightsizing recommendations

Tags:
- Environment: Production, Development
- Application: OrderService, CustomerAPI
- CostCenter: Engineering, Finance
```

## Reference Architectures

### Microservices on AWS
```
Architecture:
1. API Gateway → ALB → ECS Fargate
2. Service mesh (App Mesh) for inter-service communication
3. RDS Aurora for transactional data
4. DynamoDB for high-scale data
5. ElastiCache Redis for caching
6. S3 for object storage
7. SQS/SNS for async messaging
8. EventBridge for event routing
9. CloudWatch for monitoring
10. X-Ray for distributed tracing

.NET services:
- ASP.NET Core Web APIs
- Hangfire for background jobs
- MassTransit for messaging
- OpenTelemetry for observability
```

### Serverless Architecture
```
Components:
1. CloudFront + S3: Static website
2. API Gateway: REST/HTTP API
3. Lambda: .NET 8 functions (Native AOT)
4. DynamoDB: Data storage
5. Cognito: Authentication
6. EventBridge: Event routing
7. Step Functions: Workflows
8. SQS: Queue processing

Benefits:
- No server management
- Auto-scaling
- Pay-per-use
- High availability built-in
```

### Data Analytics Pipeline
```
Architecture:
1. Kinesis Data Streams: Ingest data
2. Kinesis Firehose: Load to S3/Redshift
3. S3: Data lake storage
4. Glue: ETL jobs, Data Catalog
5. Athena: Query S3 with SQL
6. QuickSight: Visualization
7. EMR: Big data processing (Spark)
8. Redshift: Data warehouse

For .NET:
- Kinesis Producer Library
- AWS SDK for data operations
- Glue Python/Scala jobs
```

## Best Practices Summary

### Security
- Enable MFA on root account
- Use IAM roles, not access keys
- Enable CloudTrail in all regions
- Encrypt data at rest and in transit
- Use VPC endpoints for AWS services
- Implement least privilege access
- Enable AWS Config for compliance

### Reliability
- Deploy across multiple AZs
- Use Auto Scaling for elasticity
- Implement health checks
- Use managed services when possible
- Regular backups and DR testing
- Circuit breakers and retries
- Chaos engineering (AWS FIS)

### Performance
- Use CloudFront for global content
- Enable caching at all layers
- Use read replicas for databases
- Implement async patterns
- Monitor with CloudWatch
- Use appropriate instance types
- Optimize network paths

### Cost
- Right-size resources
- Use Reserved Instances/Savings Plans
- Implement auto-scaling
- Use S3 lifecycle policies
- Delete unused resources
- Monitor with Cost Explorer
- Tag everything for attribution

## AWS CLI Common Commands

```bash
# EC2
aws ec2 describe-instances --filters "Name=tag:Environment,Values=production"
aws ec2 start-instances --instance-ids i-1234567890abcdef0

# S3
aws s3 cp file.txt s3://my-bucket/
aws s3 sync ./local-dir s3://my-bucket/backup/

# ECS
aws ecs update-service --cluster production --service api --desired-count 5

# RDS
aws rds describe-db-instances
aws rds create-db-snapshot --db-instance-id mydb --db-snapshot-id backup-2024

# Lambda
aws lambda invoke --function-name myFunction output.json

# CloudFormation
aws cloudformation deploy --template-file template.yaml --stack-name my-stack

# Secrets Manager
aws secretsmanager get-secret-value --secret-id MyAppSecret
```

## Resources

- [AWS Well-Architected Framework](https://aws.amazon.com/architecture/well-architected/)
- [AWS Architecture Center](https://aws.amazon.com/architecture/)
- [AWS Solutions Library](https://aws.amazon.com/solutions/)
- [AWS SDK for .NET](https://aws.amazon.com/sdk-for-net/)
- [AWS Cost Calculator](https://calculator.aws/)
