FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore
COPY CqrsExample/*.csproj ./CqrsExample/
RUN dotnet restore CqrsExample/CqrsExample.csproj

# copy source files only and publish
COPY CqrsExample/. ./CqrsExample/
WORKDIR /src/CqrsExample
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CqrsExample.dll"]
