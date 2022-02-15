APP_NAME="sonarqube-proxy"
docker build -f webapi.dockerfile -t "$APP_NAME" .
docker run --rm -it -p 5020:80 "$APP_NAME"