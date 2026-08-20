FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /source

COPY src/Estoque/Korp.Estoque.Api/Korp.Estoque.Api.csproj src/Estoque/Korp.Estoque.Api/
COPY src/Estoque/Korp.Estoque.Application/Korp.Estoque.Application.csproj src/Estoque/Korp.Estoque.Application/
COPY src/Estoque/Korp.Estoque.Domain/Korp.Estoque.Domain.csproj src/Estoque/Korp.Estoque.Domain/
COPY src/Estoque/Korp.Estoque.Infrastructure/Korp.Estoque.Infrastructure.csproj src/Estoque/Korp.Estoque.Infrastructure/
RUN dotnet restore src/Estoque/Korp.Estoque.Api/Korp.Estoque.Api.csproj

COPY . .
RUN dotnet publish src/Estoque/Korp.Estoque.Api/Korp.Estoque.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 8080

RUN apk add --no-cache icu-libs
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Korp.Estoque.Api.dll"]
