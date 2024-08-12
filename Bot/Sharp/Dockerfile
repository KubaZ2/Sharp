FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY Sharp.Runtime/Sharp.Runtime.csproj Sharp.Runtime/Sharp.Runtime.csproj
RUN dotnet restore Sharp.Runtime/Sharp.Runtime.csproj

COPY Bot/Mobius.ILasm/Mobius.ILASM/Mobius.ILasm.csproj Bot/Mobius.ILasm/Mobius.ILASM/Mobius.ILasm.csproj
RUN dotnet restore Bot/Mobius.ILasm/Mobius.ILASM/Mobius.ILasm.csproj

COPY Bot/Sharp/Sharp.csproj Bot/Sharp/Sharp.csproj
RUN dotnet restore Bot/Sharp/Sharp.csproj

COPY Sharp.Runtime Sharp.Runtime
COPY Bot/Mobius.ILasm/Mobius.ILASM Bot/Mobius.ILasm/Mobius.ILASM
COPY Bot/Sharp Bot/Sharp

RUN dotnet publish Bot/Sharp/Sharp.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

COPY --from=build-env /app .

ENTRYPOINT ["dotnet", "Sharp.dll"]
