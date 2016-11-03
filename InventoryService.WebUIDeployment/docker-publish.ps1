docker build -t inventoryserviceUI .
docker tag inventoryserviceUI contactsamie/inventoryserviceUI
docker push contactsamie/inventoryserviceUI
### stop the container from exiting
powershell