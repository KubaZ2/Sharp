FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src

COPY Manager/Manager.csproj Manager/Manager.csproj

RUN dotnet restore Manager/Manager.csproj

COPY Manager Manager

RUN dotnet publish Manager/Manager.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build-env /app .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Sharp.Backend.Manager.dll"]
