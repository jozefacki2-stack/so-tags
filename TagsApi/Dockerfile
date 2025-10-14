# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src/TagsApi

COPY TagsApi/*.csproj ./
RUN dotnet restore

COPY TagsApi/. .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5000

RUN mkdir -p /data
RUN adduser -D appuser && chown -R appuser:appuser /app /data
USER appuser

COPY --from=build /app/publish ./
EXPOSE 5000
ENTRYPOINT ["dotnet", "TagsApi.dll"]
