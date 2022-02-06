DAY=$(date -d "$D" '+%d')
MONTH=$(date -d "$D" '+%m')
YEAR=$(date -d "$D" '+%Y')
ver=1.$YEAR.$MONTH.$DAY
docker build -t "scholtz2/vote-coin-api:$ver-stable" -f VoteCoinApi/Dockerfile  ./
docker push "scholtz2/vote-coin-api:$ver-stable"
docker tag "scholtz2/vote-coin-api:$ver-stable" "scholtz2/vote-coin-api:latest"
docker push "scholtz2/vote-coin-api:latest"
echo "Image: scholtz2/vote-coin-api:$ver-stable"
echo "Image: scholtz2/vote-coin-api:latest"
