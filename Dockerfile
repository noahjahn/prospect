FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY . .

RUN dotnet dev-certs https -ep /tmp/aspnetcore.pfx -p 7777 --trust

RUN dotnet restore "src/Prospect.sln"

RUN dotnet publish "src/Prospect.Server.Api/Prospect.Server.Api.csproj" -c "Season 3 Release" -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

COPY --from=build /tmp/aspnetcore.pfx .

ENV Kestrel__Certificates__Default__Path="./aspnetcore.pfx"
ENV Kestrel__Certificates__Default__Password="7777"

ENTRYPOINT ["dotnet", "Prospect.Server.Api.dll"]
