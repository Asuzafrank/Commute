FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the entire solution/repository
COPY . .

# Restore the API project
RUN dotnet restore "./CommutePro.Server/CommutePro.Api.csproj"

# Build and publish
RUN dotnet publish "./CommutePro.Server/CommutePro.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Render exposes the application on port 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "CommutePro.Api.dll"]