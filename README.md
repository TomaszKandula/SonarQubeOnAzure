# SonarQube on Azure

SonarQube is an open-source platform developed by SonarSource for continuous inspection of code quality to perform automatic reviews with static analysis of code to detect bugs, code smells, and security vulnerabilities on 20+ programming languages. 

SonarQube server is available here: [Install the Server](https://docs.sonarqube.org/latest/setup/install-server/). SonarSource provides examples using a traditional installation with the zip file or Docker container using one of Docker images.

However, in a real world scenario I wanted to have SonarQube deployed on Azure Service App with SQL Database instance. A serverless approach with minimum maintenance. 

The Azure App Service is great for that because it focuses more on the application rather than the operational maintenance of the infrastructure. It provides by default:
- An SSL / TLS termination provided with the App service plan.
- A shared storage space that can reach 250 GB.
- Manual or automatic scaling according to the pricing plan.

## Prerequisites

Apart from Azure subscription:  

- **Azure Resource Group** to host service plan, service plan and storage.
- **Azure Service Plan** for Azure App Service.
- **Azure App Service on Linux** to host serverless environment.
- **Azure Storage** to host SQL Database.
- **SQL Database** hosted on Azure File Share.
- **SQL Server image** running in the container.
- **SonarQube image** running in the container.

## Deployment design

The Docker compose file of the solution consists of the SonarQube service exposing the port 9000, and an internal non-public SQL server service on the default port 1433. For data persistence, we use mounting volumes on Azure File Share that we will associate with the web app.

## 1. STEP: Azure Services

First deploy Azure services, either via Azure CLI or using Azure Portal, so the **resource group** will host:

- Azure Service Plan (at least B2 pricing tier).
- Azure App Service (Docker container and Linux O/S). We will use Docker compose.
- Azure Storage (LSR / StorageV2 will be OK).

## 2. STEP: Azure File Share

On Azure Storage, add below folders to Azure File Share:
- mssql
- sonarqube-conf
- sonarqube-data
- sonarqube-extensions
- sonarqube-bundled-plugins

## 3. STEP: Azure App Service

On Azure App Service, in the configuration, add below mappings:

| Azure File Share | Mount path
|---|---
| mssql | /var/opt/mssql
| sonarqube-conf | /opt/sonarqube/conf
| sonarqube-data | /opt/sonarqube/data
| sonarqube-extensions | /opt/sonarqube/extensions
| sonarqube-bundled-plugins | /opt/sonarqube/lib/bundled-plugins

## 4. STEP: SQL Database

The unusual step, we must initialize database manually in the Docker (local machine) and move files to Azure File Share (`mssql` folder). This unfortunately, cannot be done automatically when Docker starts.

First step is to deploy Microsoft SQL Server, docker-compose script:

```yaml
version: "3"

services:
  database_temp:
    image: mcr.microsoft.com/mssql/server:2019-latest
    container_name: mssql-server
    restart: always
    ports:
      - "5550:1433"
    volumes:
      - mssqltemp:/var/opt/mssql
    environment:
      ACCEPT_EULA: 'Y'
      MSSQL_SA_PASSWORD: <password>
      MSSQL_PID: Express
      MSSQL_COLLATION: SQL_Latin1_General_CP1_CS_AS

volumes:
  mssqltemp:
```

After executing, when SQL Server is up and running, create new database, in Azure Data Studio (or any other database management) run:

```sql
USE master
GO
IF NOT EXISTS (SELECT [name] FROM sys.databases WHERE [name] = N'<database_name>')
CREATE DATABASE <database_name>
GO
```

Next step is to go to the console and copy `mssql` folder from Docker to local machine:

`sudo docker cp <Docker_ID>:/var/opt/mssql/ /<Destination_Folder>`

Upload just two folders `.system` and `data` into `mssql` folder on the Azure File Share.

## 5. STEP: Running SonarQube

Last step is to enable Docker Compose in Azure App Service and save the below script:

```yaml
version: "3"
   
services:
  sonarqube:
    depends_on:
      - db
    image: sonarqube:7.7-community
    command: "-Dsonar.search.javaAdditionalOpts=-Dnode.store.allow_mmapfs=false"
    ports:
      - "9000:9000"
    volumes:
      - sonarqube-conf:/opt/sonarqube/conf
      - sonarqube-data:/opt/sonarqube/data
      - sonarqube-extensions:/opt/sonarqube/extensions
      - sonarqube-bundled-plugins:/opt/sonarqube/lib/bundled-plugins
    environment:
      - SONARQUBE_JDBC_URL=jdbc:sqlserver://db:1433;databaseName=<database_name>
      - SONARQUBE_JDBC_USERNAME=sa
      - SONARQUBE_JDBC_PASSWORD=<password>
      - SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true

  db:
    image: mcr.microsoft.com/mssql/server:2019-latest
    container_name: mssql-server
    volumes:
      - mssql:/var/opt/mssql
    environment:
      ACCEPT_EULA: 'Y'
      MSSQL_SA_PASSWORD: <password>
      MSSQL_PID: Express
      MSSQL_COLLATION: SQL_Latin1_General_CP1_CS_AS

volumes:
  sonarqube-conf:
     external: true
  sonarqube-data:
     external: true
  sonarqube-extensions:
     external: true
  sonarqube-bundled-plugins:
     external: true
  mssql:
    external: true
```

**Note**: please keep the same database name and password.

## End note

To prevent from `max virtual memory` error we must disable use of memory mapping in ElasticSearch, thus we use the following options:

- `-Dsonar.search.javaAdditionalOpts=-Dnode.store.allow_mmapfs=false`,
- `SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true`.

After SonarQube is up and running, log in with `admin`/`admin` credentials and change the admin password.
