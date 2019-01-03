#!/bin/bash
#
# build.sh(1)
#

[[ -n $DEBUG ]] && set -x
set -eu -o pipefail

# build parameters
readonly REGION=${AWS_DEFAULT_REGION:-"eu-central-1"}
readonly IMAGE_NAME='rolemapper-service'
readonly BUILD_NUMBER=${1:-"N/A"}
readonly BUILD_SOURCES_DIRECTORY=${2:-${PWD}}
readonly SERVICE_NAME="RolemapperService"

restore_dependencies() {
    echo "Restoring dependencies"
    dotnet restore ${SERVICE_NAME}.sln
}

run_tests() {
    echo "Running tests..."
    dotnet build -c Release ${SERVICE_NAME}.sln
    dotnet test --logger:"trx;LogFileName=testresults.trx" --results-directory ${BUILD_SOURCES_DIRECTORY}/output ${SERVICE_NAME}.WebApi.Tests/${SERVICE_NAME}.WebApi.Tests.csproj
}

publish_binaries() {
    echo "Publishing binaries..."
    dotnet publish -c Release -o ${BUILD_SOURCES_DIRECTORY}/output/app ${SERVICE_NAME}.WebApi/${SERVICE_NAME}.WebApi.csproj
}

build_container_image() {
    echo "Building container image..."
    docker build -t ${IMAGE_NAME} .
}

push_container_image() {
    echo "Login to docker..."
    $(aws ecr get-login --no-include-email)

    account_id=$(aws sts get-caller-identity --output text --query 'Account')
    image_name="${account_id}.dkr.ecr.${REGION}.amazonaws.com/ded/${IMAGE_NAME}:${BUILD_NUMBER}"

    echo "Tagging container image..."
    docker tag ${IMAGE_NAME}:latest ${image_name}

    echo "Pushing container image to ECR..."
    docker push ${image_name}
}

push_container_image_dockercloud() {
    echo "Login to docker..."
    docker login -u ${DOCKER_USERNAME} -p ${DOCKER_PASSWORD}

    image_name="dfdsdk/rolemapper-service:${BUILD_NUMBER}"

    echo "Tagging container image..."
    docker tag ${IMAGE_NAME}:latest ${image_name}

    echo "Pushing container image to ECR..."
    docker push ${image_name}
}


cd ./src

restore_dependencies
run_tests
publish_binaries

cd ..

build_container_image

if [[ "${BUILD_NUMBER}" != "N/A" ]]; then
    push_container_image
    push_container_image_dockercloud
fi