ver=1.0.0
docker build -t "scholtz2/vote-coin-api:$ver-stable" -f VoteCoinApi/Dockerfile  ./
docker push "scholtz2/vote-coin-api:$ver-stable"
echo "Image: scholtz2/vote-coin-api:$ver-stable"
