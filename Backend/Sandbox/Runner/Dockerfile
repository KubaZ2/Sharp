ARG DOTNET_SDK_VERSION=9.0
ARG DOTNET_RUNTIME_VERSION=9.0.0

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:$DOTNET_SDK_VERSION AS build-env
WORKDIR /src

COPY Sharp.Runtime/Sharp.Runtime.csproj Sharp.Runtime/Sharp.Runtime.csproj
RUN dotnet restore Sharp.Runtime/Sharp.Runtime.csproj

COPY Backend/Sandbox/Shared/Shared.csproj Backend/Sandbox/Shared/Shared.csproj
RUN dotnet restore Backend/Sandbox/Shared/Shared.csproj

COPY Backend/Sandbox/Runner/Runner.csproj Backend/Sandbox/Runner/Runner.csproj
RUN dotnet restore Backend/Sandbox/Runner/Runner.csproj

COPY Sharp.Runtime Sharp.Runtime
COPY Backend/Sandbox/Shared Backend/Sandbox/Shared
COPY Backend/Sandbox/Runner Backend/Sandbox/Runner

RUN dotnet publish Backend/Sandbox/Runner/Runner.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:$DOTNET_RUNTIME_VERSION AS runtime

COPY --from=build-env /app /app

FROM pwn.red/jail

COPY --from=runtime / /srv

COPY Backend/Sandbox/Runner/start.sh /srv/app/run

ENV JAIL_TIME=10
ENV JAIL_PIDS=100
ENV JAIL_MEM=100M
ENV JAIL_TMP_SIZE=52428800
