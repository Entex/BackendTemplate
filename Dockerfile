FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/BackendTemplate.Domain/*.csproj src/BackendTemplate.Domain/
COPY src/BackendTemplate.Application/*.csproj src/BackendTemplate.Application/
COPY src/BackendTemplate.Infrastructure/*.csproj src/BackendTemplate.Infrastructure/
COPY src/BackendTemplate.ServiceDefaults/*.csproj src/BackendTemplate.ServiceDefaults/
COPY src/BackendTemplate.Api/*.csproj src/BackendTemplate.Api/
RUN dotnet restore src/BackendTemplate.Api
COPY src/ src/
RUN dotnet publish src/BackendTemplate.Api -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir /app/data && chown "$APP_UID" /app/data
ENV ConnectionStrings__Database="Data Source=/app/data/backend.db"
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "BackendTemplate.Api.dll"]
