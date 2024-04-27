resource "azurerm_resource_group" "project" {
    name = "Project"
    location = "eastus"
    tags = {
      Environment = "qa"
      Application = "workshop"
      Createdby = "terraform"
    }
}

resource "azurerm_kubernetes_cluster" "project" {
    name = "project"
    location = azurerm_resource_group.project.location
    resource_group_name = azurerm_resource_group.project.name
    dns_prefix = "project"
    default_node_pool {
        name = "default"
        node_count = 1
        vm_size = "Standard_D2_v2"
    }
    identity {
      type = "SystemAssigned"
    }
    tags = {
      Environment = "qa"
      Application = "project"
      Createdby = "terraform"
    }
}

resource "null_resource" "kubeconfig" {
    provisioner "local-exec" {
    command = "az aks get-credentials --resource-group ${azurerm_resource_group.project.name} --name ${azurerm_kubernetes_cluster.project.name} --overwrite-existing"
  }
}