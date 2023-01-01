# SonarQube on Azure Virtual Machine

## Prerequisites

Apart from Azure subscription you will need Azure Resource Group to host Virtual Machine. In this example I will also use external SQL Database (for my own convenience), but it is not necessary.

## Deployment design

The Docker compose file of the solution consists of the SonarQube service exposing the port 9000.

We will also use NGINX to proxy HTTP and HTTPS requests to a SonarQube web server.

## Azure Virtual Machine

Add Azure Virtual Machine to your newly created Azure Resource Group. I prefer CentOS 7.9 from OpenLogic, but any other Linux will do. The size that work just fine is `Standard B2s (2 vcpus, 4 GiB memory)`.

After creating VM: 

1. Navigate to the VM resource group and select item with a type `Public IP address`, then navigate to `configuration` and setup domain name, so the ful address follows schema: `<YOUR_DOMAIN>.westeurope.cloudapp.azure.com`. We will use it in the configuration.
1. Log into it and setup root password: `sudo passwd root`.

Once that is done, let's install unnecessary ingredients.

### GIT

`sudo yum install git`

### DOCKER

```
curl -sSL https://get.docker.com/ | CHANNEL=stable sh
systemctl enable --now docker
```

### DOCKER-COMPOSE

```
COMPOSE_VERSION=$(curl -s https://api.github.com/repos/docker/compose/releases/latest | grep 'tag_name' | cut -d\" -f4)
sudo sh -c "curl -L https://github.com/docker/compose/releases/download/${COMPOSE_VERSION}/docker-compose-'uname -s'-'uname -m' > /usr/local/bin/docker-compose"
sudo chmod +x /usr/local/bin/docker-compose
sudo sh -c "curl -L https://raw.githubusercontent.com/docker/compose/${COMPOSE_VERSION}/contrib/completion/bash/docker-compose > /etc/bash_completion.d/docker-compose"
sudo ln -s /usr/local/bin/docker-compose /usr/bin/docker-compose
```

Now we may check if everything is OK, following command will display version number:

`docker-compose -v`

### ENABLING SELINUX

Run command: `sudo docker info | grep selinux`. If nothing is printed on the console, then:

```
sudo touch /etc/docker/daemon.json
sudo nano /etc/docker/daemon.json
```

And paste below:

```json
{
  "selinux-enabled": true
}
```

### SONAR PROPERTIES

Create folder `sonarqube` in `/opt` folder:

```
cd /opt
sudo mkdir sonarqube
```

Then add the following folders:

```
cd sonarqube
sudo mkdir conf
sudo mkdir data
sudo mkdir logs
sudo mkdir extensions
sudo mkdir bundled-plugins
```

Once that done, create and edit `sonar.properties`:

```
sudo touch sonar.properties
sudo nano sonar.properties
```

Paste below configuration:

```
# https://www.oracle.com/technetwork/java/javase/8u111-relnotes-3124969.html - Disabling of HTTP tunneling basic authentication by JDK 8
sonar.web.javaOpts=-Xmx512m -Xms128m -XX:+HeapDumpOnOutOfMemoryError -Djdk.http.auth.tunneling.disabledSchemes=""

sonar.web.host=0.0.0.0
sonar.web.port=9000

http.proxyHost=127.0.0.1
http.proxyPort=80
```

Our SonarQube installation will be working on all IP addresses associated with the server, accepting incoming HTTP on port 9000. Therefore, we will set proxy to our machine (127.0.0.1) on port 80 (for SonarQube web service).

### SSL

Before we can start with NGINX, we must obtain SSL certificates. If we do not have it yet, we may use Let's Encrypt free of charge:

```
sudo yum install epel-release
sudo yum install certbot python2-certbot-nginx
sudo certbot --nginx
```

Provide with the answers for all the questions from certbot. If everything is alright, then new certificate can be found at:

Certificate should be saved:	`/etc/letsencrypt/live/<YOUR_DOMAIN>/fullchain.pem`
Key should be saved:			`/etc/letsencrypt/live/<YOUR_DOMAIN>/privkey.pem`

Please note that `live` folder holds symlinks, the files are stored in `archive` folder.

### NGINX

Let's copy SSL certification files to the NGINX:

```
sudo cp /etc/letsencrypt/archive/<DOMAIN>/fullchain.pem /opt/nginx/fullchain.pem
sudo cp /etc/letsencrypt/archive/<DOMAIN>/privkey.pem /opt/nginx/privkey.pem
```

Now create configuration file, execute:

```
sudo touch sonarqube.conf
sudo nano sonarqube.conf
```

Paste below code that will handle HTTP and HTTPS requests:

```
server {
  listen 80;
  server_name tokansonar.westeurope.cloudapp.azure.com;
  access_log /var/log/nginx/access.log;

  location / {
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_connect_timeout 90;
    proxy_send_timeout    90;
    proxy_read_timeout    90;
    proxy_buffering off;
    proxy_cache_valid 200 30m;
    proxy_cache_valid 404 1m;

    client_max_body_size    10m;
    client_body_buffer_size 128k;

    proxy_pass http://sonarqube:9000;
  }
}

server {
  listen 443 ssl;
  server_name <HOST_DOMAIN_NAME>;
  
  ssl_certificate /etc/nginx/fullchain.pem;
  ssl_certificate_key /etc/nginx/privkey.pem;
  
  ssl_session_cache shared:le_nginx_SSL:10m;
  ssl_session_timeout 1440m;
  ssl_session_tickets off;

  ssl_protocols TLSv1.2 TLSv1.3;
  ssl_prefer_server_ciphers off;

  ssl_ciphers "ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:DHE-RSA-AES128-GCM-SHA256:DHE-RSA-AES256-GCM-SHA384";

  access_log /var/log/nginx/access.log;

  location / {
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_connect_timeout 90;
    proxy_send_timeout    90;
    proxy_read_timeout    90;
    proxy_buffering off;
    proxy_cache_valid 200 30m;
    proxy_cache_valid 404 1m;

    client_max_body_size    10m;
    client_body_buffer_size 128k;

    proxy_pass http://sonarqube:9000;
  }
}
```

### DOCKER-COMPOSE

Navigate to `/opt/sonarqube` and create docker-compose file:

```
sudo touch docker-compose.yml
sudo nano docker-compose.yml
```

We will use two components only latest NGINX and SonarQube 9.8:

```
version: "3"

services:
  nginx:
    image: nginx:latest
    container_name: nginx
    ports:
      - 80:80
      - 443:443
    volumes:
      - /opt/nginx/fullchain.pem:/etc/nginx/fullchain.pem:ro
      - /opt/nginx/privkey.pem:/etc/nginx/privkey.pem:ro
      - /opt/nginx/sonarqube.conf:/etc/nginx/conf.d/sonarqube.conf:ro
    networks:
      - sonarnet
    restart: always

  sonarqube:
    image: sonarqube:9.8-community
    command: "-Dsonar.search.javaAdditionalOpts=-Dnode.store.allow_mmap=false"
    container_name: sonarqube
    hostname: sonarqube
    networks:
      - sonarnet   
    ports:
      - "9000:9001"
    volumes:
      - /opt/sonarqube/conf:/opt/sonarqube/conf:z
      - /opt/sonarqube/data:/opt/sonarqube/data:z
      - /opt/sonarqube/logs:/opt/sonarqube/logs:z
      - /opt/sonarqube/extensions:/opt/sonarqube/extensions:z
      - /opt/sonarqube/bundled-plugins:/opt/sonarqube/lib/bundled-plugins:z
    environment:
      - SONARQUBE_JDBC_URL=<JDBC_CONNECTION_STRING>
      - SONARQUBE_JDBC_USERNAME=<USERNAME>
      - SONARQUBE_JDBC_PASSWORD=<PASSWORD>
      - SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true
    restart: unless-stopped

networks:
  sonarnet:
    driver: bridge
```

Now we can run commands:

```
sudo docker-compose pull
sudo docker-compose up -d
```

To look what is happening, execute:

`sudo docker-compose logs`

We should see NGINX and Sonarqube up and running with no errors. SonarQube should only prompt warning for admin account set to default `admin/admin` (scheme: login/password). Navigate to your domain and change your password.

## FINAL THOUGHTS

Unlike installing SonarQube on an Azure AppService, installing on an Azure VM is much more work. However, in return we have much more control/flexibility over our installation.
