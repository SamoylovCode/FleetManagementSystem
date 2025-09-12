# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Копируем файл проекта (он находится в той же папке)
COPY FleetManagementSystemApp.csproj ./
RUN dotnet restore "FleetManagementSystemApp.csproj"

# Копируем весь код из текущей папки
COPY . ./
WORKDIR /source

# Публикуем приложение
RUN dotnet publish "FleetManagementSystemApp.csproj" -c Release -o /app/publish

# Этап запуска
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 80
ENTRYPOINT ["dotnet", "FleetManagementSystemApp.dll"]