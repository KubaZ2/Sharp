ARG DOTNET_RUNTIME_VERSION=9.0.0
ARG OS=ubuntu
ARG OS_VERSION=24.04
ARG OS_CODENAME=noble

FROM --platform=$BUILDPLATFORM $OS:$OS_CODENAME AS build-env
WORKDIR /runtime
ARG DOTNET_RUNTIME_VERSION
ARG OS_CODENAME
ARG TARGETARCH
ARG BUILDARCH

RUN apt update -y && apt install -y git

RUN git clone --depth 1 --branch v$DOTNET_RUNTIME_VERSION https://github.com/dotnet/runtime.git .

# Asserts fail when running in pwn.red/jail, so we need to remove them
RUN sed -i 214d src/coreclr/pal/src/misc/cgroup.cpp
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
        ln -s /usr/bin/llvm-objcopy-14 /usr/bin/llvm-objcopy && \
        apt install -y debootstrap lld && \
        ./eng/install-native-dependencies.sh && \
        ./eng/common/cross/build-rootfs.sh $DOTNET_ARCH $OS_CODENAME && \
        ROOTFS_DIR=/runtime/.tools/rootfs/$DOTNET_ARCH ./build.sh clr -c Checked -a $DOTNET_ARCH --cross; \
    fi

FROM scratch AS storage

COPY --from=build-env /runtime/artifacts/bin/coreclr/linux.*.*/libclrjit.so /runtime/libclrjit.so
