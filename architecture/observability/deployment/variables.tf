variable "region" {
  description = "AWS region"
  type        = string
}

variable "name" {
  description = "Name prefix applied to the resources."
  type        = string
  default     = "signoz"
}

variable "subnet_tag_key" {
  description = "Tag key used to find the existing subnets."
  type        = string
  default     = "Name"
}

variable "subnet_tag_value" {
  description = "Tag value used to find the existing subnets. Wildcards such as app* are allowed."
  type        = string
  default     = "app"
}