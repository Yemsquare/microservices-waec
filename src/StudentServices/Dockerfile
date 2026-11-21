# ==========================================
# BUILD STAGE
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project file
COPY src/StudentServices/StudentServices.csproj /src/StudentServices/StudentServices.csproj

# Restore dependencies
RUN dotnet restore /src/StudentServices/StudentServices.csproj

# Copy remaining source files
COPY src/StudentServices/ /src/StudentServices/

# Verify project file exists
RUN ls -la /src/StudentServices/

# Publish
RUN dotnet publish /src/StudentServices/StudentServices.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# ==========================================
# RUNTIME STAGE
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "StudentServices.dll"]