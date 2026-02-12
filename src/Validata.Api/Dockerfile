FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY src/Validata.Core/Validata.Core.csproj src/Validata.Core/
COPY src/Validata.Application/Validata.Application.csproj src/Validata.Application/
COPY src/Validata.Infrastructure/Validata.Infrastructure.csproj src/Validata.Infrastructure/
COPY src/Validata.Integration/Validata.Integration.csproj src/Validata.Integration/
COPY src/Validata.Api/Validata.Api.csproj src/Validata.Api/
RUN dotnet restore src/Validata.Api/Validata.Api.csproj

COPY src/Validata.Core/ src/Validata.Core/
COPY src/Validata.Application/ src/Validata.Application/
COPY src/Validata.Infrastructure/ src/Validata.Infrastructure/
COPY src/Validata.Integration/ src/Validata.Integration/
COPY src/Validata.Api/ src/Validata.Api/

RUN dotnet publish src/Validata.Api/Validata.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "Validata.Api.dll"]
