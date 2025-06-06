# Usar la imagen oficial de Microsoft para .NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Usar la imagen SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo del proyecto y restaurar dependencias
COPY ["PokemonWebApp.csproj", "./"]
RUN dotnet restore "PokemonWebApp.csproj"

# Copiar todo el código fuente
COPY . .

# Build de la aplicación
RUN dotnet build "PokemonWebApp.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "PokemonWebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app

# Crear directorio para logs
RUN mkdir -p /app/Logs

# Copiar los archivos publicados
COPY --from=publish /app/publish .

# Configurar variables de entorno para producción
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Punto de entrada
ENTRYPOINT ["dotnet", "PokemonWebApp.dll"]