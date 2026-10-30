locals {
  subnet_ids = sort(data.aws_subnets.selected.ids)
  vpc_ids    = distinct([for s in data.aws_subnet.selected : s.vpc_id])
}
