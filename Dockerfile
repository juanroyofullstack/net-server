# Etapa 1: Build (compilación)
FROM mcr.microsoft.com/dotnet/sdk:10 AS build
WORKDIR /src

# Copiar archivos del proyecto
COPY ["NetCosmosServer.csproj", "./"]
RUN dotnet restore "NetCosmosServer.csproj"

# Copiar código fuente
COPY . .

# Compilar en Release
RUN dotnet build "NetCosmosServer.csproj" -c Release -o /app/build

# Publicar
RUN dotnet publish "NetCosmosServer.csproj" -c Release -o /app/publish

# Etapa 2: Runtime (imagen final)
FROM mcr.microsoft.com/dotnet/aspnet:10
WORKDIR /app

# Copiar binarios compilados desde etapa build
COPY --from=build /app/publish .

# Exponer puerto (debe coincidir con tu app)
EXPOSE 5083

# Variables de entorno para Cosmos (reemplazar en producción)
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5083

# Arrancar la app
ENTRYPOINT ["dotnet", "NetCosmosServer.dll"]
