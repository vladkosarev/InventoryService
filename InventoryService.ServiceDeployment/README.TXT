docker build -t inventoryservice:v1.95 .
docker run -t -i -d   --name=inventoryservice inventoryservice:v1.95
(SET EXTERNAL PORT TO 8099)


FROM microsoft/aspnet

RUN apt-get update && apt-get install mono-4.0-service -y

RUN mkdir -p /app
COPY ./bin/Debug /app
 
WORKDIR /app
CMD [ "mono",  "./InventoryService.ServiceDeployment.exe" ]
 
EXPOSE 8099



-bash-4.2# docker login --username=xxxxxxx --email=xxxxxxxx.xxxxxxx@gmail.com

docker tag inventoryservice:v1.95 contactsamie/inventoryservice:v1
docker push contactsamie/inventoryservice:v1


EITHER FROM UPPER FOLDER

docker-compose up -d

OR

docker build -t inventoryservice-client:v5 .
docker run -t -i -d -name=inventoryservice-client  inventoryservice-client:v5
docker run -t -i -d --link inventoryservice  --name=inventoryservice-client  inventoryservice-client:v5
docker attach inventoryservice-client



docker-compose up -d
is same as 
docker build -t commander commander
docker run -d --name frontend -e VAR1=value -p 8081:8081
   --link backend:redis commander

   Each service needs to point to an image or build directory; all other keywords (links, ports, environment, restart) correspond to docker options.
docker-compose up -d builds images if needed.
docker-compose ps shows running containers.

docker-compose stop && docker-compose rm -v 
stops and removes all containers








push example



Windows PowerShell
Copyright (C) 2016 Microsoft Corporation. All rights reserved.

PS D:\GITHUB\InventoryService\Src\InventoryService.ServiceDeployment> docker build -t inventoryservice .
Sending build context to Docker daemon 43.17 MB
PS InventoryService.ServiceDeployment> docker tag inventoryservice contactsamie/inventoryservice
PS InventoryService.ServiceDeployment> docker push contactsamie/inventoryservice
The push refers to a repository [docker.io/contactsamie/inventoryservice]