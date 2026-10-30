terraform {
  backend "s3" {
    bucket       = "014858193168-observability"
    key          = "signoz/terraform.tfstate"
    region       = "af-south-1"
    encrypt      = true
    use_lockfile = true
  }
}
