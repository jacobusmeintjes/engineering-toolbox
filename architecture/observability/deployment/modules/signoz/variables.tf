variable "name" {
  description = "Name prefix applied to the resources in this module."
  type        = string
}

variable "subnet_tag_key" {
  description = "Tag key used to find the existing subnets."
  type        = string
}

variable "subnet_tag_value" {
  description = "Tag value used to find the existing subnets. Wildcards such as app* are allowed."
  type        = string
}
