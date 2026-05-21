# ── Build Stage ──
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY MyApp.API.sln .
COPY MyApp.API/MyApp.API.csproj           MyApp.API/
COPY MyApp.Application/MyApp.Application.csproj  MyApp.Application/
COPY MyApp.Domain/MyApp.Domain.csproj     MyApp.Domain/
COPY MyApp.Infrastructure/MyApp.Infrastructure.csproj MyApp.Infrastructure/

# Restore dependencies
RUN dotnet restore MyApp.API/MyApp.API.csproj

# Copy all source files
COPY . .

# Publish release build
RUN dotnet publish MyApp.API/MyApp.API.csproj -c Release -o /app/publish

# ── Runtime Stage ──
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "MyApp.API.dll"]
