docker build -t inventoryservice .
docker tag inventoryservice contactsamie/inventoryservice
docker push contactsamie/inventoryservice
### stop the container from exiting
powershell