FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY Mobius.ILasm/Mobius.ILASM/Mobius.ILasm.csproj Mobius.ILasm/Mobius.ILASM/Mobius.ILasm.csproj
RUN dotnet restore Mobius.ILasm/Mobius.ILASM/Mobius.ILasm.csproj

COPY Sharp/Sharp.csproj Sharp/Sharp.csproj
RUN dotnet restore Sharp/Sharp.csproj

COPY Mobius.ILasm/Mobius.ILASM Mobius.ILasm/Mobius.ILASM
COPY Sharp Sharp

RUN dotnet publish Sharp/Sharp.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

COPY --from=build-env /app .

ENTRYPOINT ["dotnet", "Sharp.dll"]
