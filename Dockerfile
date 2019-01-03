FROM microsoft/dotnet:2.1.4-aspnetcore-runtime-stretch-slim

WORKDIR /app
COPY ./output/app ./

ENTRYPOINT [ "dotnet", "RolemapperService.WebApi.dll" ]