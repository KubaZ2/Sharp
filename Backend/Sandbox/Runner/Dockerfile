ARG DOTNET_SDK_VERSION=8.0
ARG DOTNET_RUNTIME_VERSION=8.0.7

FROM mcr.microsoft.com/dotnet/sdk:$DOTNET_SDK_VERSION AS build-env
WORKDIR /src

COPY Sandbox/Shared/Shared.csproj Sandbox/Shared/Shared.csproj
RUN dotnet restore Sandbox/Shared/Shared.csproj

COPY Sandbox/Runner/Runner.csproj Sandbox/Runner/Runner.csproj
RUN dotnet restore Sandbox/Runner/Runner.csproj

COPY Sandbox/Shared Sandbox/Shared
COPY Sandbox/Runner Sandbox/Runner

RUN dotnet publish Sandbox/Runner/Runner.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:$DOTNET_RUNTIME_VERSION AS runtime

COPY --from=build-env /app /app

FROM pwn.red/jail

COPY --from=runtime / /srv

# RUN ln -s /app/Sharp.Backend.Sandbox.Runner /srv/app/run

COPY Sandbox/Runner/start.sh /srv/app/run

ENV JAIL_TIME=10
ENV JAIL_PIDS=100
ENV JAIL_MEM=100M
ENV JAIL_TMP_SIZE=52428800
