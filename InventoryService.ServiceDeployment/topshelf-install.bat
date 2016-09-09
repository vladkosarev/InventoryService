
cd /d %~dp0
InventoryService.ServiceDeployment.exe stop
InventoryService.ServiceDeployment.exe uninstall
InventoryService.ServiceDeployment.exe install 
InventoryService.ServiceDeployment.exe start
PAUSE