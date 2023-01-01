# SonarQube on Azure

SonarQube is a SonarSource platform for continuous inspection of code quality to perform automatic reviews with static code analysis to detect various bugs, code smells, and security vulnerabilities on 20+ programming languages. 

SonarQube server is available here: [Install the Server](https://docs.sonarqube.org/latest/setup/install-server/). SonarSource provides examples using a standard installation with the zip file or Docker container using Docker images.

However, what about hosting SonarQube on Microsoft Azure Cloud? There are at least two possibilities, I tried both:

1. Host SonarQube using Azure AppService (with Docker-Compose).
2. Host SonarQube using Azure Virtual Machine (Linux in my case).

Navigate to ___ for SonarQube on Azure AppService.

Navigate to ___ for SonarQube on Azure VM.

## Proxy API to SonarQube API

Additionally, a [NET 6 application](https://github.com/TomaszKandula/SonarQubeOnAzure/tree/master/SonarQubeProxy) serves as a proxy to SonarQube API. Having _man-in-the-middle_ is to abstract away SonarQube API, so users do not have to query SonarQube API directly using the secret key in the request.

The web application requires Docker, and currently we deploy it to the Azure App Service via CI/CD. Production requires merging a code to the master branch. SonarQube code analysis is trigerred when a code is merged from the custom branch to the development branch (dev).

There are two endpoints:
1. GetMetrics - returns badge from SonarQube server for given project name and metric type. All badges have the same style.
2. GetQualityGate - returns large quality gate badge from SonarQube server for given project name.

List of metric types:
1. bugs
1. code_smells
1. coverage
1. duplicated_lines_density
1. ncloc
1. sqale_rating
1. alert_status
1. reliability_rating
1. security_rating
1. sqale_index
1. vulnerabilities
