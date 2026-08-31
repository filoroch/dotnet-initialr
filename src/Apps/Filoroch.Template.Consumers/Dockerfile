FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Apps/Filoroch.Template.Consumers/Filoroch.Template.Consumers.csproj
RUN dotnet publish src/Apps/Filoroch.Template.Consumers/Filoroch.Template.Consumers.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Filoroch.Template.Consumers.dll"]
