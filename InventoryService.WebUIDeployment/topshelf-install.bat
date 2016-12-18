
cd /d %~dp0
InventoryService.WebUIDeployment.exe stop
InventoryService.WebUIDeployment.exe uninstall
InventoryService.WebUIDeployment.exe install 
InventoryService.WebUIDeployment.exe start
PAUSE