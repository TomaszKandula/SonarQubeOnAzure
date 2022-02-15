# SonarQube on Azure

SonarQube is a SonarSource platform for continuous inspection of code quality to perform automatic reviews with static code analysis to detect various bugs, code smells, and security vulnerabilities on 20+ programming languages. 

SonarQube server is available here: [Install the Server](https://docs.sonarqube.org/latest/setup/install-server/). SonarSource provides examples using a standard installation with the zip file or Docker container using Docker images.

However, I wanted to have SonarQube deployed on Azure Service App with Azure SQL Database instance (a real-world scenario). A serverless approach with minimum maintenance. For once, I am accustomed to Azure, I have used it for a very long time, so this would be my natural choice.

Furthermore, the Azure App Service is excellent because it focuses more on the application than the infrastructure’s operational maintenance. It provides by default:

1. An SSL / TLS termination provided with the App service plan.
1. It has a shared storage space that can reach 250 GB.
1. Manual or automatic scaling according to the pricing plan.

## Prerequisites

Apart from Azure subscription:

- **Azure Resource Group** to host service plan, service plan and storage.
- **Azure Service Plan** for Azure App Service.
- **Azure App Service on Linux** to host serverless environment.
- **Azure Storage** to host SonarQube files.
- **SQL Server** hosted on Azure.
- **SQL Database** hosted on Azure.
- **SonarQube image** running in the container.

## Deployment design

The Docker compose file of the solution consists of the SonarQube service exposing the port 9000.

We will use mounting volumes on Azure File Share and an external SQL database hosted on Azure for data persistence.

## STEP 1: Azure Services

First deploy Azure services, either via Azure CLI or using Azure Portal, so that the **resource group** will host:

- Azure Service Plan (at least B2 pricing tier).
- Azure App Service (Docker container and Linux O/S) - we will use Docker compose.
- Azure Storage (LSR / StorageV2 will be OK).
- Azure SQL Server and database (basic tier will suffice).

## STEP 2: Azure File Share

On Azure Storage, add the below **folders** to Azure File Share:
- sonarqube-conf
- sonarqube-data
- sonarqube-logs
- sonarqube-extensions
- sonarqube-bundled-plugins

## STEP 3: Azure App Service

On Azure App Service, in the configuration, add below mappings:

| Azure File Share | Mount path |
|---|--- |
| sonarqube-conf | /opt/sonarqube/conf |
| sonarqube-data | /opt/sonarqube/data |
| sonarqube-logs | /opt/sonarqube/logs |
| sonarqube-extensions | /opt/sonarqube/extensions |
| sonarqube-bundled-plugins | /opt/sonarqube/lib/bundled-plugins |

## STEP 4: SQL Database

Setup Azure SQL server and add SQL Database. A basic tier with 2 GB storage will suffice. Use connection string for JDBC; however, please do not place username and password in the given connection string. Credentials should be passed via environment variables in Docker compose a script.

Once the database is setup, create login and user, and assign permissions, for example:

Execute on master only:
```sql
CREATE LOGIN SonarAccess WITH PASSWORD = '<password>';
```

**Note**: Password must comply with [Password policy in Azure AD](https://docs.microsoft.com/en-us/previous-versions/azure/jj943764(v=azure.100)).

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

## STEP 5: Running SonarQube

The last step is to enable Docker Compose in Azure App Service and save the below script:

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
      - sonarqube-logs:/opt/sonarqube/logs
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
  sonarqube-logs:
     external: true
  sonarqube-extensions:
     external: true
  sonarqube-bundled-plugins:
     external: true
```

**Note**: because SonarQube will migrate the database, the first run may take some time. From my experience: SonarQube application that uses Azure SQL Database on basic tier usually take a few minutes to take off.

To prevent from `max virtual memory` error we must disable use of memory mapping in ElasticSearch, thus we use the following options:

- `-Dsonar.search.javaAdditionalOpts=-Dnode.store.allow_mmap=false`,
- `SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true`.

## STEP 6: First login

After SonarQube is up and running, log in with:

- login: `admin`,
- password: `admin`.

And change the admin password so nobody else will log in to your SonarQube service. Additionally, one can navigate to the below address to quick check installed plugins:

`https://<your_app_name>.azurewebsites.net/api/plugins/installed`

For SonarQube 8.9 LTS, there should be around 15 plugins installed by default:
- C#,
- JavaScript,
- Kotlin,
- PHP (among others).

Other plugins can be easily added; just go to Administration -> Marketplace.

