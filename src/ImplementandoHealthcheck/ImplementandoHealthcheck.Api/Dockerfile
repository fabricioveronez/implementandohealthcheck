FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ImplementandoHealthcheck.Api/ImplementandoHealthcheck.Api.csproj ImplementandoHealthcheck.Api/
RUN dotnet restore ImplementandoHealthcheck.Api/ImplementandoHealthcheck.Api.csproj
COPY . .
WORKDIR /src/ImplementandoHealthcheck.Api
RUN dotnet build ImplementandoHealthcheck.Api.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ImplementandoHealthcheck.Api.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ImplementandoHealthcheck.Api.dll"]
