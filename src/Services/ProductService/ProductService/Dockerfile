## Multi-stage Dockerfile for ProductService (net9.0)
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy entire repo (build context should be repo root)
COPY . .

# Restore, build, and publish ProductService
WORKDIR /src/src/Services/ProductService/ProductService
RUN dotnet restore ProductService.csproj
RUN dotnet build ProductService.csproj -c Release -o /app/build
RUN dotnet publish ProductService.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProductService.dll"]
