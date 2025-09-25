## Multi-stage Dockerfile for OrderService (net9.0)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
WORKDIR /src/src/Services/OrderService/OrderService
RUN dotnet restore OrderService.csproj
RUN dotnet build OrderService.csproj -c Release -o /app/build
RUN dotnet publish OrderService.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderService.dll"]
