# SonarQube on Azure

SonarQube is an open-source platform developed by SonarSource for continuous inspection of code quality to perform automatic reviews with static code analysis to detect bugs, code smells, and security vulnerabilities on 20+ programming languages. 

SonarQube server is available here: [Install the Server](https://docs.sonarqube.org/latest/setup/install-server/). SonarSource provides examples using a standard installation with the zip file or Docker container using Docker images.

**However, I wanted to have SonarQube deployed on Azure Service App with Azure SQL Database instance (a real-world scenario). A serverless approach with minimum maintenance.**

The Azure App Service is excellent because it focuses more on the application rather than the operational maintenance of the infrastructure. It provides by default:
- An SSL / TLS termination provided with the App service plan.
- A shared storage space that can reach 250 GB.
- Manual or automatic scaling according to the pricing plan.

## Prerequisites

Apart from Azure subscription:  

- **Azure Resource Group** to host service plan, service plan and storage.
- **Azure Service Plan** for Azure App Service.
- **Azure App Service on Linux** to host serverless environment.
- **Azure Storage** to host SQL Database.
- **SQL Server** hosted on Azure.
- **SQL Database** hosted on Azure.
- **SonarQube image** running in the container.

## Deployment design

The Docker compose file of the solution consists of the SonarQube service exposing the port 9000. For data persistence, we use mounting volumes on Azure File Share that we will associate with the web app. We use external SQL database, hosted on Azure.

## 1. STEP: Azure Services

First deploy Azure services, either via Azure CLI or using Azure Portal, so the **resource group** will host:

- Azure Service Plan (at least B2 pricing tier).
- Azure App Service (Docker container and Linux O/S) - we will use Docker compose.
- Azure Storage (LSR / StorageV2 will be OK).
- Azure SQL Server and database (basic tier will suffice).

## 2. STEP: Azure File Share

On Azure Storage, add below folders to Azure File Share:
- sonarqube-conf
- sonarqube-data
- sonarqube-extensions
- sonarqube-bundled-plugins

## 3. STEP: Azure App Service

On Azure App Service, in the configuration, add below mappings:

| Azure File Share | Mount path
|---|---
| sonarqube-conf | /opt/sonarqube/conf
| sonarqube-data | /opt/sonarqube/data
| sonarqube-extensions | /opt/sonarqube/extensions
| sonarqube-bundled-plugins | /opt/sonarqube/lib/bundled-plugins

## 4. STEP: SQL Database

Setup Azure SQL server and add SQL Database. A basic tier with 2 GB storage will suffice. Use connection string for JDBC; however, please do not place username and password in the given connection string. Credentials should be passed via environment variables in Docker compose a script. 

Once the database is setup, create login and user, and assign permissions, for example:

Execute on master only:
```sql
CREATE LOGIN SonarAccess WITH PASSWORD = '<password>';
```

Note: Password must comply with [Password policy in Azure AD](https://docs.microsoft.com/en-us/previous-versions/azure/jj943764(v=azure.100)).

Execute on target database:
```sql
CREATE USER SonarAccess FROM LOGIN SonarAccess; 

EXEC sp_addrolemember 'db_datareader', SonarAccess
EXEC sp_addrolemember 'db_datawriter', SonarAccess

GRANT CREATE TABLE to SonarAccess
GRANT ALTER on schema::dbo to SonarAccess
GRANT DELETE on schema::dbo to SonarAccess
GRANT INSERT on schema::dbo to SonarAccess
GRANT SELECT on schema::dbo to SonarAccess
GRANT UPDATE on schema::dbo to SonarAccess
GRANT REFERENCES on schema::dbo to SonarAccess
```

## 5. STEP: Running SonarQube

Last step is to enable Docker Compose in Azure App Service and save the below script:

```yaml
version: "3"
   
services:
  sonarqube:
    image: sonarqube:8.9-community
    command: "-Dsonar.search.javaAdditionalOpts=-Dnode.store.allow_mmap=false"
    ports:
      - "9000:9000"
    volumes:
      - sonarqube-conf:/opt/sonarqube/conf
      - sonarqube-data:/opt/sonarqube/data
      - sonarqube-extensions:/opt/sonarqube/extensions
      - sonarqube-bundled-plugins:/opt/sonarqube/lib/bundled-plugins
    environment:
      - SONARQUBE_JDBC_URL=<connection_string>
      - SONARQUBE_JDBC_USERNAME=<user_name>
      - SONARQUBE_JDBC_PASSWORD=<password>
      - SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true

volumes:
  sonarqube-conf:
     external: true
  sonarqube-data:
     external: true
  sonarqube-extensions:
     external: true
  sonarqube-bundled-plugins:
     external: true
```

**Note**: first run may take some time as the SonarQube will migrate database. In case of Azure SQL Database and basic tier, it took few minutes for application to take off.

## End note

To prevent from `max virtual memory` error we must disable use of memory mapping in ElasticSearch, thus we use the following options:

- `-Dsonar.search.javaAdditionalOpts=-Dnode.store.allow_mmap=false`,
- `SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true`.

After SonarQube is up and running, log in with `admin`/`admin` credentials and change the admin password.
