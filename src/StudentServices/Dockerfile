FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/StudentServices.csproj src/StudentServices/
RUN dotnet restore src/StudentServices.csproj
COPY src/StudentServices/ src/StudentServices/
RUN dotnet publish src/StudentServices.csproj -c Release -o /app/publish --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StudentServices.dll"]