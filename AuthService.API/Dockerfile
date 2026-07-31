FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project file first
# This improves Docker layer caching
COPY ["AuthService.API.csproj", "./"]

# Restore dependencies
RUN dotnet restore "AuthService.API.csproj"

# Copy source code
COPY . .

# Build and publish
RUN dotnet publish "AuthService.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

# ASP.NET Core listens on port 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Copy published application
COPY --from=build /app/publish .

# Container listens on port 8080
EXPOSE 8080

ENV DOTNET_USE_POLLING_FILE_WATCHER=false

# Start application
ENTRYPOINT ["dotnet", "AuthService.API.dll"]
