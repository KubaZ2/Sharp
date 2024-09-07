ARG DOTNET_SDK_VERSION=8.0
ARG DOTNET_RUNTIME_VERSION=8.0.7
ARG OS=ubuntu
ARG OS_VERSION=24.04
ARG OS_CODENAME=noble
ARG MINUS_PREFIXED_PLATFORM_IF_NOT_AMD64

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

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet-buildtools/prereqs:$OS-$OS_VERSION$MINUS_PREFIXED_PLATFORM_IF_NOT_AMD64 AS runtime-build-env
WORKDIR /runtime
ARG DOTNET_RUNTIME_VERSION
ARG OS_CODENAME
ARG TARGETARCH
ARG BUILDARCH

RUN git clone --depth 1 --branch v$DOTNET_RUNTIME_VERSION https://github.com/dotnet/runtime.git .

# Asserts fail when running in pwn.red/jail, so we need to remove them
RUN sed -i 247d src/coreclr/pal/src/misc/cgroup.cpp
RUN sed -i 666d src/coreclr/pal/src/sync/cs.cpp

RUN set -e; trap 'cat /runtime/.tools/rootfs/x64/debootstrap/debootstrap.log' EXIT; \
    if [ $TARGETARCH = $BUILDARCH ]; then \
        ./build.sh clr -c Checked; \
    else \
        case $TARGETARCH in \
            arm64) BINUTILS_ARCH=aarch64 DOTNET_ARCH=arm64 ;; \
            arm) BINUTILS_ARCH=arm DOTNET_ARCH=arm ;; \
            amd64) BINUTILS_ARCH=x86-64 DOTNET_ARCH=x64 ;; \
            x86) BINUTILS_ARCH=i686 DOTNET_ARCH=x86 ;; \
            *) echo "Unsupported target architecture: $TARGETARCH" && exit 1 ;; \
        esac && \
        sudo apt update -y && sudo apt install -y qemu-user-static binfmt-support debootstrap binutils-$BINUTILS_ARCH-linux-gnu && \
        ln -s /usr/bin/llvm-objcopy-14 /usr/bin/llvm-objcopy && \
        sudo ./eng/common/cross/build-rootfs.sh $DOTNET_ARCH $OS_CODENAME && \
        ROOTFS_DIR=/runtime/.tools/rootfs/$DOTNET_ARCH ./build.sh clr -c Checked -a $DOTNET_ARCH --cross; \
    fi

FROM mcr.microsoft.com/dotnet/runtime:$DOTNET_RUNTIME_VERSION-$OS_CODENAME AS runtime
ARG DOTNET_RUNTIME_VERSION

COPY --from=build-env /app /app

COPY --from=runtime-build-env /runtime/artifacts/bin/coreclr/linux.*.*/libclrjit.so /usr/share/dotnet/shared/Microsoft.NETCore.App/$DOTNET_RUNTIME_VERSION/

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
