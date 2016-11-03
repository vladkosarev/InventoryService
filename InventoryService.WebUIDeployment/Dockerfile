FROM microsoft/aspnet

ENV SOME_ENV 1.10.1

RUN apt-get update && apt-get install mono-4.0-service -y

RUN mkdir -p /app
COPY ./bin/Debug /app
 
WORKDIR /app
CMD [ "mono",  "./InventoryService.WebUIDeployment.exe" ]
 
EXPOSE 8099
EXPOSE 9200
