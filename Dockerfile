FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

RUN apk update
RUN apk add bash
RUN apk add jq

WORKDIR /app

COPY *.sln .

COPY ./Redis.OM.Playground.Api/Redis.OM.Playground.Api.csproj ./Redis.OM.Playground.Api/Redis.OM.Playground.Api.csproj

RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore \
  --runtime linux-musl-x64

COPY . .

ARG buildConfiguration=Release

ARG version=0.0.0

RUN --mount=type=cache,target=/root/.nuget/packages dotnet build \
  *.sln \
  -c ${buildConfiguration} \
  --no-restore

RUN --mount=type=cache,target=/root/.nuget/packages dotnet publish \
    Redis.OM.Playground.Api/Redis.OM.Playground.Api.csproj \
    -c Release \
    -o out \
    --no-restore \
    -r linux-musl-x64 \
    --self-contained true \
    /p:PublishSingleFile=true

RUN mv /app/out/appsettings.json /app/out/template.appsettings.json && \
  rm -f /app/out/appsettings*.json && \
  jq -e 'del(.RedisConfiguration)' /app/out/template.appsettings.json > /app/out/appsettings.json &&

FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-alpine

WORKDIR /app

RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app

COPY --from=build /app/out/ ./

RUN mv Redis.OM.Playground.Api run-service

USER dotnetuser

ENTRYPOINT ["./run-service"]