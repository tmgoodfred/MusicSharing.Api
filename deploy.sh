#!/bin/bash
set -e

# Expect these provided by GitHub Actions SSH invocation
export CONNECTION_STRING
export JWT_KEY
# Email settings already exported when calling this script (hierarchical names)

echo "Starting deployment process..."

PORT=5000

if [ -z "$CONNECTION_STRING" ]; then
  echo "ERROR: CONNECTION_STRING not set"
  exit 1
fi
if [ -z "$JWT_KEY" ]; then
  echo "ERROR: JWT_KEY not set"
  exit 1
fi
# Only PASS is critical secret
if [ -z "$EMAIL__Smtp__Pass" ]; then
  echo "ERROR: EMAIL__Smtp__Pass (SendGrid API key) not set"
  exit 1
fi

cd /mnt/user/appdata/musicsharing-api

echo "Stopping prior container..."
docker stop musicsharing-api-container > /dev/null 2>&1 || true
docker rm musicsharing-api-container > /dev/null 2>&1 || true

echo "Building image..."
docker build -t musicsharing-api-image . > /dev/null

echo "Running container..."
docker run -d \
  --name musicsharing-api-container \
  -p $PORT:5000 \
  -e ASPNETCORE_URLS="http://+:5000" \
  -e ConnectionStrings__DefaultConnection="$CONNECTION_STRING" \
  -e Jwt__Key="$JWT_KEY" \
  -e Email__Smtp__Host="${EMAIL__Smtp__Host}" \
  -e Email__Smtp__Port="${EMAIL__Smtp__Port}" \
  -e Email__Smtp__From="${EMAIL__Smtp__From}" \
  -e Email__Smtp__User="${EMAIL__Smtp__User}" \
  -e Email__Smtp__Pass="${EMAIL__Smtp__Pass}" \
  -e Email__Smtp__EnableSsl="${EMAIL__Smtp__EnableSsl}" \
  --restart unless-stopped \
  -v /mnt/user/music-files/artwork:/mnt/user/music-files/artwork \
  -v /mnt/user/music-files/audio:/mnt/user/music-files/audio \
  -v /mnt/user/music-files/profile-pictures:/mnt/user/music-files/profile-pictures \
  musicsharing-api-image

echo "Container started. Checking health..."
sleep 5
if curl -s --head http://localhost:$PORT >/dev/null; then
  echo "Service responding on port $PORT"
else
  echo "WARNING: Service not responding yet. Logs:"
  docker logs musicsharing-api-container || true
fi