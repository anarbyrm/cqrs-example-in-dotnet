FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore
COPY CqrsExample/*.csproj ./CqrsExample/
RUN dotnet restore CqrsExample/CqrsExample.csproj

# copy everything and publish
COPY . .
WORKDIR /src/CqrsExample
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "CqrsExample.dll"]
