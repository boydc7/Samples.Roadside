#!/usr/bin/env bash

# USAGE
#	bash docker-up-local.sh
#
#	TO RESET LOCAL ENV (including a rebuild):
#		bash docker-up-local.sh reset
#
#	TO START THE API (in addition to the default of just starting dependencies):
#		bash docker-up-local.sh api
#
#	TO START THE API AND REBUILD/RESET LOCAL ENV
#		bash docker-up-local.sh reset api
#
#	TO START THE API AND REBUILD CODE ONLY (without resetting local dependencies):
#		bash docker-up-local.sh build api
#
# 	To stop:
# 		docker-compose -f docker-compose.yml down
#

ROADSIDE_COMPOSE_ARGS="--no-deps --detach"

if [ "$1" = "reset" ];
then
	echo "Cleaning dependencies and rebuilding..."
	ROADSIDE_COMPOSE_ARGS="$ROADSIDE_COMPOSE_ARGS --force-recreate --renew-anon-volumes --build"
elif [ "$1" = "build" ];
then
	echo "Rebuilding..."
	ROADSIDE_COMPOSE_ARGS="$ROADSIDE_COMPOSE_ARGS --build"
else 
	echo "Starting dependencies..." 
fi

docker compose -f docker-compose.yml up $ROADSIDE_COMPOSE_ARGS roadside-es roadside-sql roadside-zookeeper roadside-kafka

esIsGreen=0

until [ $esIsGreen -gt 0 ]
do
	if health="$(curl -fsSL "http://localhost:1792/_cat/health?h=status")"; then
		health="$(echo "$health" | sed 's/^[[:space:]]+|[[:space:]]+$//g')" # trim whitespace (otherwise we'll have "green ")
		if [ "$health" = 'green' ]; then
			((esIsGreen++))
			echo ""
			echo "Elastic is green..."
			echo ""
		fi
	else
		sleep 4
	fi
done

echo "ROADSIDE dependency services are ready..."

if [ "$1" = "api" ] || [ "$2" = "api" ]; 
then
	echo "api specified, starting API..." 
	docker compose -f docker-compose.yml up $ROADSIDE_COMPOSE_ARGS roadside-api
	echo "API is ready..."
fi

echo ""
echo "All done"
echo ""
