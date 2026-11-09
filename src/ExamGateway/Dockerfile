FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/ExamGateway/ExamGateway.csproj src/ExamGateway/
RUN dotnet restore src/ExamGateway/ExamGateway.csproj
COPY src/ExamGateway/ src/ExamGateway/
# Run restore again after copying the complete project so every transitive package
# is present before publishing. This avoids intermittent NETSDK1064 cache misses.
RUN dotnet publish src/ExamGateway/ExamGateway.csproj -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ExamGateway.dll"]
