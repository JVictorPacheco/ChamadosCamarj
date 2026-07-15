FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /app

# Copiar arquivos de projeto
COPY . .

# Restaurar dependências
RUN dotnet restore

# Build
RUN dotnet build -c Release

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

COPY --from=build /app/src/ChamadosCamarj.WebApi/bin/Release/net9.0 .

# Health check
HEALTHCHECK --interval=10s --timeout=5s --start-period=30s --retries=5 \
  CMD curl -f http://localhost:5000/health || exit 1

EXPOSE 5000

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "ChamadosCamarj.WebApi.dll"]
