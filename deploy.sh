#!/bin/bash
set -e

export CONNECTION_STRING
export JWT_KEY

echo "Starting deployment process..."

# Configuration
PORT=5000

# Use the CONNECTION_STRING from the environment
if [ -z "$CONNECTION_STRING" ]; then
    echo "ERROR: CONNECTION_STRING environment variable is not set!"
    exit 1
fi

echo "Using port $PORT for deployment"

# Navigate to the repository directory
cd /mnt/user/appdata/musicsharing-api

# Check if the port is already in use by other containers or processes
echo "Checking for containers using port $PORT..."
container_id=$(docker ps --filter "publish=$PORT" --format "{{.ID}}")

if [ ! -z "$container_id" ]; then
    echo "Port $PORT is in use by container $container_id. Stopping and removing it..."
    docker stop $container_id || true
    docker rm $container_id || true
fi

# Also kill any non-docker process as a backup
pid=$(lsof -t -i:$PORT || true)
if [ ! -z "$pid" ]; then
    echo "Also found regular process $pid using port $PORT. Killing it..."
    kill -9 $pid || true
fi

echo "Pulling latest changes from git..."
git pull --quiet

echo "Stopping existing container if running..."
docker stop musicsharing-api-container > /dev/null 2>&1 || true
docker rm musicsharing-api-container > /dev/null 2>&1 || true

echo "Waiting for container to fully stop and release port..."
sleep 5

# Double-check and kill any lingering process on port again just in case
pid=$(lsof -t -i:$PORT || true)
if [ ! -z "$pid" ]; then
    echo "Warning: Process $pid still using port $PORT, killing it..."
    kill -9 $pid || true
fi

echo "Building Docker image..."
docker build -t musicsharing-api-image . > /dev/null

echo "Extra cleanup: Stopping and removing containers publishing port $PORT..."
docker ps -q --filter "publish=$PORT" | xargs -r docker stop
docker ps -q --filter "publish=$PORT" | xargs -r docker rm

echo "JWT_KEY is: $JWT_KEY"

echo "Running container on port $PORT..."
docker run -d \
    --name musicsharing-api-container \
    -p $PORT:5000 \
    -e ASPNETCORE_URLS="http://+:5000" \
    -e ConnectionStrings__DefaultConnection="$CONNECTION_STRING" \
    -e Jwt__Key="$JWT_KEY" \
    --restart unless-stopped \
    -v /mnt/user/music-files/artwork:/mnt/user/music-files/artwork \
    -v /mnt/user/music-files/audio:/mnt/user/music-files/audio \
    musicsharing-api-image

echo "Deployment completed successfully!"
echo "Website should be available at http://$(hostname -I | awk '{print $1}'):$PORT"

# Wait a moment for the container to start
sleep 5

# Check if the container is running
if docker ps | grep -q musicsharing-api-container; then
    echo "Container is running successfully."
    
    # Test if the website is responding
    if curl -s --head http://localhost:$PORT > /dev/null; then
        echo "Website is responding correctly on localhost."
    else
        echo "WARNING: Website is not responding. Check container logs:"
        docker logs musicsharing-api-container
    fi
else
    echo "ERROR: Container failed to start. Check logs:"
    docker logs musicsharing-api-container || echo "Container not found."
fi