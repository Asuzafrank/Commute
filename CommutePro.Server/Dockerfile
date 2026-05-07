FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project files
COPY . .

# Restore (adjust path to your API project)
RUN dotnet restore "./CommutePro.Server/CommutePro.API.csproj"

# Publish
RUN dotnet publish "./CommutePro.Server/CommutePro.API.csproj" -c Release -o /app/publish

# Runtime

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Set environment
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CommutePro.API.dll"]