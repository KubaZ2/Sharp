ARG DOTNET_SDK_VERSION=8.0
ARG DOTNET_RUNTIME_VERSION=8.0.7
ARG OS_CODENAME=noble

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:$DOTNET_SDK_VERSION AS build-env
WORKDIR /src

COPY Sharp.Runtime/Sharp.Runtime.csproj Sharp.Runtime/Sharp.Runtime.csproj
RUN dotnet restore Sharp.Runtime/Sharp.Runtime.csproj

COPY Backend/Sandbox/Shared/Shared.csproj Backend/Sandbox/Shared/Shared.csproj
RUN dotnet restore Backend/Sandbox/Shared/Shared.csproj

COPY Backend/Sandbox/Asm/Asm.csproj Backend/Sandbox/Asm/Asm.csproj
RUN dotnet restore Backend/Sandbox/Asm/Asm.csproj

COPY Sharp.Runtime Sharp.Runtime
COPY Backend/Sandbox/Shared Backend/Sandbox/Shared
COPY Backend/Sandbox/Asm Backend/Sandbox/Asm

RUN dotnet publish Backend/Sandbox/Asm/Asm.csproj -c Release -o /app

# 'docker buildx use default' is needed to use local 'ghcr.io/kubaz2/sharp/checked-jit' image
FROM ghcr.io/kubaz2/sharp/checked-jit:$DOTNET_RUNTIME_VERSION AS checked-jit

FROM mcr.microsoft.com/dotnet/runtime:$DOTNET_RUNTIME_VERSION-$OS_CODENAME AS runtime
ARG DOTNET_RUNTIME_VERSION

COPY --from=build-env /app /app

COPY --from=checked-jit /runtime/libclrjit.so /usr/share/dotnet/shared/Microsoft.NETCore.App/$DOTNET_RUNTIME_VERSION/

FROM pwn.red/jail

COPY --from=runtime / /srv

COPY Backend/Sandbox/Asm/start.sh /srv/app/run

ENV JAIL_TIME=10
ENV JAIL_PIDS=100
ENV JAIL_MEM=100M
ENV JAIL_TMP_SIZE=52428800
ENV JAIL_PORT=6000

ENV JAIL_ENV_DOTNET_TieredCompilation=0
ENV JAIL_ENV_DOTNET_JitDisasmAssemblies=_
ENV JAIL_ENV_DOTNET_JitDisasm=*
ENV JAIL_ENV_DOTNET_JitStdOutFile=/tmp/asm
