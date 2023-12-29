FROM mcr.microsoft.com/dotnet/aspnet:5.0.6-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0.301-buster-slim AS build
COPY ["src/", "src/"]
RUN dotnet restore "src/Csb.BigMom.App/Csb.BigMom.App.csproj"
RUN dotnet build "src/Csb.BigMom.App/Csb.BigMom.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Csb.BigMom.App/Csb.BigMom.App.csproj" -c Release -o /app/publish -r debian.10-x64 -p:PublishReadyToRun=true

FROM node:14-buster as front
COPY ["src/Csb.BigMom.App/ClientApp", "/src"]
#COPY ["src/Csb.BigMom.App/bigmomapp.pfx", "/https/bigmomapp.pfx"]
#COPY ["src/Csb.BigMom.App/bigmomapp.pem", "/usr/local/share/ca-certificates/bigmomapp.pem"]
#RUN update-ca-certificates
WORKDIR /src
RUN npm install --pure-lockfile
#RUN npm run build:prod
RUN npm run build

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=front /src/dist/bigmom-app ./ClientApp/dist

# The curl CLI is used by Docker to health check the container.
RUN mkdir -p /etc/apt/apt.conf.d/ \
 #&& echo "Acquire::http::Proxy \"$HTTP_PROXY\";\nAcquire::https::Proxy \"$HTTPS_PROXY\";" > /etc/apt/apt.conf.d/proxy.conf \
 && apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/* \
 && rm -rf /etc/apt/apt.conf.d/

ARG CSB_GID=2000
ARG BIGMOM_UID=5000

RUN groupadd -g $CSB_GID csb && \
    useradd -U -G csb -d /home/bigmom -m -s /bin/nologin -u $BIGMOM_UID bigmom && \
    chown -R $BIGMOM_UID:$CSB_GID /home/bigmom .
USER $BIGMOM_UID:$CSB_GID

ENTRYPOINT ["dotnet", "Csb.BigMom.App.dll"]