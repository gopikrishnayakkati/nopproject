# output k8s cluster name
output "name" {
    value = azurerm_kubernetes_cluster.project.name
}