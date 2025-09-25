## Multi-stage Dockerfile for ShippingService (net9.0)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
WORKDIR /src/src/Services/ShippingService/ShippingService
RUN dotnet restore ShippingService.csproj
RUN dotnet build ShippingService.csproj -c Release -o /app/build
RUN dotnet publish ShippingService.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShippingService.dll"]
