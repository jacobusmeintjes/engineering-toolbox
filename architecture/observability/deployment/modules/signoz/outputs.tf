output "vpc_id" {
  description = "ID of the VPC that contains the selected subnets."
  value       = local.vpc_ids[0]
}

output "subnet_ids" {
  description = "IDs of the existing subnets selected by tag."
  value       = local.subnet_ids
}

output "security_group_id" {
  description = "ID of the security group for the SigNoz tasks."
  value       = aws_security_group.signoz.id
}
