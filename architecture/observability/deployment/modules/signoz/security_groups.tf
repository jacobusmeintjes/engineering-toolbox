resource "aws_security_group" "signoz" {
  name        = "${var.name}-tasks"
  description = "SigNoz ECS tasks: private traffic between the SigNoz components only"
  vpc_id      = local.vpc_ids[0]

  tags = { Name = "${var.name}-tasks" }
}

# Members of this group can reach each other on any port. Nothing else is allowed in,
# so the group is not reachable from the internet. Access from you is added later.
resource "aws_vpc_security_group_ingress_rule" "self" {
  security_group_id            = aws_security_group.signoz.id
  referenced_security_group_id = aws_security_group.signoz.id
  ip_protocol                  = "-1"
  description                  = "Traffic between SigNoz components"
}

# Outbound is needed to pull images and write logs.
resource "aws_vpc_security_group_egress_rule" "all" {
  security_group_id = aws_security_group.signoz.id
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "-1"
  description       = "Outbound traffic"
}
