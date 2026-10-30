module "signoz" {
  source = "./modules/signoz"

  name             = var.name
  subnet_tag_key   = var.subnet_tag_key
  subnet_tag_value = var.subnet_tag_value
}
