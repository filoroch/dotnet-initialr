FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Apps/Filoroch.Template.Api/Filoroch.Template.Api.csproj
RUN dotnet publish src/Apps/Filoroch.Template.Api/Filoroch.Template.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Filoroch.Template.Api.dll"]
