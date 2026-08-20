FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /source

COPY src/Faturamento/Korp.Faturamento.Api/Korp.Faturamento.Api.csproj src/Faturamento/Korp.Faturamento.Api/
COPY src/Faturamento/Korp.Faturamento.Application/Korp.Faturamento.Application.csproj src/Faturamento/Korp.Faturamento.Application/
COPY src/Faturamento/Korp.Faturamento.Domain/Korp.Faturamento.Domain.csproj src/Faturamento/Korp.Faturamento.Domain/
COPY src/Faturamento/Korp.Faturamento.Infrastructure/Korp.Faturamento.Infrastructure.csproj src/Faturamento/Korp.Faturamento.Infrastructure/
RUN dotnet restore src/Faturamento/Korp.Faturamento.Api/Korp.Faturamento.Api.csproj

COPY . .
RUN dotnet publish src/Faturamento/Korp.Faturamento.Api/Korp.Faturamento.Api.csproj \
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
ENTRYPOINT ["dotnet", "Korp.Faturamento.Api.dll"]
