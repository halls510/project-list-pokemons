# Etapa Base: Ambiente de Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# Etapa Build: Ambiente de Build com SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Argumento para configurar o modo de compila��o (padr�o: Release)
ARG BUILD_CONFIGURATION=Release

# Copiar e restaurar depend�ncias
COPY ["project-list-pokemons.Api/project-list-pokemons.Api.csproj", "project-list-pokemons.Api/"]
RUN dotnet restore "./project-list-pokemons.Api/project-list-pokemons.Api.csproj"

# Copiar o restante do c�digo e compilar
COPY . .
WORKDIR "/src/project-list-pokemons.Api"
RUN dotnet build "./project-list-pokemons.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Etapa Publish: Publica��o da Aplica��o
FROM build AS publish
RUN dotnet publish "./project-list-pokemons.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Etapa Final: Imagem de Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copiar os artefatos publicados
COPY --from=publish /app/publish .

# Copiar a pasta Resources para a imagem final
COPY ["project-list-pokemons.Api/Resources", "./Resources"]

# Comando de inicializa��o
ENTRYPOINT ["dotnet", "project-list-pokemons.Api.dll"]