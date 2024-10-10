ARG DOTNET_RUNTIME_VERSION=8.0.7
ARG OS=ubuntu
ARG OS_VERSION=24.04
ARG OS_CODENAME=noble
ARG MINUS_PREFIXED_PLATFORM_IF_NOT_AMD64

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet-buildtools/prereqs:$OS-$OS_VERSION$MINUS_PREFIXED_PLATFORM_IF_NOT_AMD64 AS build-env
WORKDIR /runtime
ARG DOTNET_RUNTIME_VERSION
ARG OS_CODENAME
ARG TARGETARCH
ARG BUILDARCH

RUN git clone --depth 1 --branch v$DOTNET_RUNTIME_VERSION https://github.com/dotnet/runtime.git .

# Asserts fail when running in pwn.red/jail, so we need to remove them
RUN sed -i 247d src/coreclr/pal/src/misc/cgroup.cpp
RUN sed -i 666d src/coreclr/pal/src/sync/cs.cpp

RUN if [ $TARGETARCH = $BUILDARCH ]; then \
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

FROM --platform=$BUILDPLATFORM busybox:latest AS storage

COPY --from=build-env /runtime/artifacts/bin/coreclr/linux.*.*/libclrjit.so /runtime/libclrjit.so
