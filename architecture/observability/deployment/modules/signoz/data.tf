data "aws_subnets" "selected" {
  filter {
    name   = "tag:${var.subnet_tag_key}"
    values = [var.subnet_tag_value]
  }
}

data "aws_subnet" "selected" {
  for_each = toset(data.aws_subnets.selected.ids)

  id = each.value
}
