#!/bin/bash
set -e

echo "Starting deployment process..."

# Configuration
PORT=5000
SSL_PORT=5001
PFX_PASSWORD="SuperSecurePassword123"
PFX_OUTPUT="/mnt/user/appdata/letsencrypt/etc/letsencrypt/live/tgoodfred.com/cert.pfx"
PFX_PATH="/etc/letsencrypt/live/tgoodfred.com/cert.pfx"

# Use the CONNECTION_STRING from the environment
if [ -z "$CONNECTION_STRING" ]; then
    echo "ERROR: CONNECTION_STRING environment variable is not set!"
    exit 1
fi

echo "Using fixed ports $PORT and $SSL_PORT for deployment"

# Navigate to the repository directory
cd /mnt/user/appdata/musicsharing-api

# Check if ports are already in use by other containers or processes
for port in $PORT $SSL_PORT; do
    echo "Checking for containers using port $port..."
    container_id=$(docker ps --filter "publish=$port" --format "{{.ID}}")

    if [ ! -z "$container_id" ]; then
        echo "Port $port is in use by container $container_id. Stopping and removing it..."
        docker stop $container_id || true
        docker rm $container_id || true
    fi

    # Also kill any non-docker process as a backup
    pid=$(lsof -t -i:$port || true)
    if [ ! -z "$pid" ]; then
        echo "Also found regular process $pid using port $port. Killing it..."
        kill -9 $pid || true
    fi
done

echo "Pulling latest changes from git..."
git pull --quiet

echo "Stopping existing container if running..."
docker stop musicsharing-api-container > /dev/null 2>&1 || true
docker rm musicsharing-api-container > /dev/null 2>&1 || true

echo "Waiting for container to fully stop and release ports..."
sleep 5

# Double-check and kill any lingering process on ports again just in case
for port in $PORT $SSL_PORT; do
    pid=$(lsof -t -i:$port || true)
    if [ ! -z "$pid" ]; then
        echo "Warning: Process $pid still using port $port, killing it..."
        kill -9 $pid || true
    fi
done

echo "Removing old PFX certificate if it exists..."
rm -f "$PFX_OUTPUT"

echo "Generating new PFX certificate..."
openssl pkcs12 -export \
  -out "$PFX_OUTPUT" \
  -inkey /mnt/user/appdata/letsencrypt/etc/letsencrypt/live/tgoodfred.com/privkey.pem \
  -in /mnt/user/appdata/letsencrypt/etc/letsencrypt/live/tgoodfred.com/fullchain.pem \
  -passout pass:"$PFX_PASSWORD"

echo "Building Docker image..."
docker build -t musicsharing-api-image . > /dev/null

echo "Extra cleanup: Stopping and removing containers publishing ports $PORT and $SSL_PORT..."
docker ps -q --filter "publish=$PORT" | xargs -r docker stop
docker ps -q --filter "publish=$PORT" | xargs -r docker rm
docker ps -q --filter "publish=$SSL_PORT" | xargs -r docker stop
docker ps -q --filter "publish=$SSL_PORT" | xargs -r docker rm

echo "Running container on ports $PORT and $SSL_PORT..."
docker run -d \
    --name musicsharing-api-container \
    -p $PORT:5000 \
    -p $SSL_PORT:5001 \
    -v /mnt/user/appdata/letsencrypt/etc/letsencrypt:/etc/letsencrypt \
    -e ASPNETCORE_URLS="http://+:5000;https://+:5001" \
    -e CERT_PATH="$PFX_PATH" \
    -e CERT_PASSWORD="$PFX_PASSWORD" \
    -e ASPNETCORE_ConnectionStrings__DefaultConnection="$CONNECTION_STRING" \
    --restart unless-stopped \
    musicsharing-api-image

echo "Deployment completed successfully!"
echo "Website should be available at https://$(hostname -I | awk '{print $1}'):$SSL_PORT"

# Wait a moment for the container to start
sleep 5

# Check if the container is running
if docker ps | grep -q musicsharing-api-container; then
    echo "Container is running successfully."
    
    # Test if the website is responding
    if curl -sk --head https://localhost:$SSL_PORT > /dev/null; then
        echo "Website is responding correctly on localhost."
    else
        echo "WARNING: Website is not responding. Check container logs:"
        docker logs musicsharing-api-container
    fi
else
    echo "ERROR: Container failed to start. Check logs:"
    docker logs musicsharing-api-container || echo "Container not found."
fi