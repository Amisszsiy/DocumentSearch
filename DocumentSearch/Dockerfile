FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DocumentSearch/DocumentSearch.csproj DocumentSearch/
RUN dotnet restore DocumentSearch/DocumentSearch.csproj

COPY DocumentSearch/ DocumentSearch/
WORKDIR /src/DocumentSearch
RUN dotnet publish DocumentSearch.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DocumentSearch.dll"]
