# SigNoz on AWS

This directory holds the Terraform for running self-hosted SigNoz on AWS. This
document lists the components SigNoz needs and their minimum resources.

Sources: [SigNoz Docker install](https://signoz.io/docs/install/docker/),
[SigNoz resource planning](https://signoz.io/docs/setup/capacity-planning/community/resources-planning/).
Re-check them before sizing, as versions and guidance change.

## SigNoz components

| Component | Purpose | Ports |
|-----------|---------|-------|
| SigNoz (`signoz/signoz`) | UI, API, query service and alerting | 8080 |
| SigNoz OTel collector (`signoz/signoz-otel-collector`) | Receives OTLP telemetry and writes it to ClickHouse | 4317 (gRPC), 4318 (HTTP) |
| ClickHouse | Telemetry store for traces, metrics and logs | 8123, 9000 |
| ClickHouse Keeper | Coordinates ClickHouse | 2181 |
| PostgreSQL | SigNoz metadata | 5432 |
| Schema migrator (one-shot job) | Creates and upgrades the ClickHouse schemas | - |

Keep every port internal to the VPC and reach the UI (8080) through a tunnel.
Only applications inside the VPC send telemetry to 4317/4318.

## Minimum resources

The stack runs as ECS services on AWS Fargate, so there are no EC2 instances or
EKS clusters to manage. Fargate task sizes are fixed combinations of CPU and
memory, so each component below is rounded up to the nearest valid size.

### Evaluation: minimal footprint

| Requirement | Minimum |
|-------------|---------|
| Total memory across tasks | 4 GB (more is recommended for real workloads) |
| Tasks | One each of SigNoz, collector, ClickHouse, Keeper and PostgreSQL |
| Storage | An EBS volume for ClickHouse sized for your retention and data volume |

### Production: per-component sizing

| Component | Tasks | CPU per task | Memory per task | Fargate size |
|-----------|-------|--------------|-----------------|--------------|
| OTel collector | 3 | 4 vCPU | 16 GiB | 4 vCPU / 16 GiB |
| ClickHouse | 2 shards | 16 vCPU | 32 GiB (varies with data volume) | 16 vCPU / 32 GiB |
| SigNoz core | 1 | 4 vCPU | 8 GiB | 4 vCPU / 8 GiB |
| PostgreSQL | 1 | 2 vCPU | 8 GiB | Managed RDS recommended |
| ClickHouse Keeper | 3 | 2 vCPU | 8 GiB | 2 vCPU / 8 GiB |

Totals: about 56 vCPU and 152 GiB of memory.

- ClickHouse storage must be sized for your retention policy and expected data
  volume, with room to grow.
- The CPU and memory figures come from SigNoz's guidance for Kubernetes
  nodes; confirm they hold on Fargate under real load.

## AWS infrastructure

| Resource | Notes |
|----------|-------|
| VPC, subnets, security groups | Existing private subnets, found by tag (`Name = app`); no internet gateway or public access; reach SigNoz through a tunnel |
| ECS cluster | Fargate launch type; one service per component, with a task definition each |
| ECR or public registry access | Container images for SigNoz, the collector, ClickHouse, Keeper and migrator |
| Persistent storage | ClickHouse and Keeper need durable disks. Use ECS-managed EBS volumes attached to Fargate tasks; avoid EFS for ClickHouse data |
| RDS for PostgreSQL | Recommended for the metadata store in production |
| Service discovery | Cloud Map (ECS Service Connect) so tasks reach each other by name, such as `clickhouse` |
| Load balancers | Internal only, for the UI and OTLP ingestion; not internet-facing |
| Configuration | ECS has no bind mounts, so collector and ClickHouse config files must be baked into images or delivered through environment variables or secrets |
| IAM roles | Task execution role (image pull, logs, secrets) and task roles |
| CloudWatch Logs | Log group for each task |
| Secrets | Secrets Manager or SSM Parameter Store, for example the SigNoz JWT secret and database passwords |
| Migrations | One-shot ECS tasks for the schema migrator, run before the services start |
| Backups and monitoring | Optional but recommended for production, for example EBS snapshots |

## Usage

Run these from this directory. `terraform.tfvars` holds the input values (such
as `region`) and is git-ignored.

```shell
terraform fmt -check -recursive
terraform init
terraform validate
terraform plan -var-file="terraform.tfvars"
```
