#!/bin/bash

KUBERNETES_SERVICE_KAFKA_BOOTSTRAP_SERVERS=localhost:9092 \
KUBERNETES_SERVICE_KAFKA_GROUP_ID=kubernetes-consumer \
KUBERNETES_SERVICE_KAFKA_ENABLE_AUTO_COMMIT=false \
dotnet watch --project ./../src/K8sJanitor.WebApi/K8sJanitor.WebApi.csproj run
