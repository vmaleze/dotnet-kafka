#!/bin/bash
docker run --rm --network dotnet-kafka_default -v /var/run/docker.sock:/var/run/docker.sock -i -t wurstmeister/kafka /bin/bash