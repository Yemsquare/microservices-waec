FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Services/StudentService/StudentService.csproj src/Services/StudentService/
RUN dotnet restore src/Services/StudentService/StudentService.csproj

COPY src/Services/StudentService/ src/Services/StudentService/
RUN dotnet publish src/Services/StudentService/StudentService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "StudentService.dll"]
