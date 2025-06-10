# Updating data custom bootstrap components via npm
Power Shell:

cd C:\Users\pervo\source\repos\FleetManagementSystemApp\FleetManagementSystemApp
sass --watch wwwroot/scss/custom.scss wwwroot/css/bootstrap-custom.css --load-path=node_modules
sass --watch wwwroot/scss/custom.scss wwwroot/css/bootstrap-custom.css --load-path=node_modules --load-path=wwwroot/scss

# MS SQL

(localdb)\mssqllocaldb
По умолчанию LocalDB создает файлы базы данных MDF в каталоге C:/Users/<user>.

# Migrations

CLI:
- dotnet ef migrations add InitialCreate
- dotnet ef database update
- dotnet ef migrations remove

PM> PACKAGE MANAGE CONSOLE:
- Add-Migration {название_миграции}
- Update-Database {название_последней_успешной_миграции}
- Update-Database -Migration "Предыдущая_миграция" - откатить последнюю примененную миграцию
- Remove-Migration -Force - Удаление файлов миграции, даже если она уже в БД (важно: миграция не откатит БД!)
- Remove-Migration - удаление последней НЕ применённой миграции
- Чтобы накатить миграции на боевую БД, прописать в application.json боевую строку к бд и выполнить шаг 2.
https://docs.microsoft.com/ru-ru/ef/core/managing-schemas/migrations/?tabs=vs

Пример безопасной очистки в dev:
1. Убедитесь, что все миграции применены
dotnet ef database update
2. Удалите папку Migrations
rm -rf Migrations/
3. Создайте новую базовую миграцию
dotnet ef migrations add InitialBaseline
dotnet ef database update

# Swagger

https://localhost:7103/swagger/index.html
https://localhost:7103/swagger/v1/swagger.json

______________________________________________

# Identity

Все вызовы Generate*TokenAsync → Confirm*Async должны быть до любых операций, меняющих пароль или профиль.

______________________________________________

# ВРЕМЕННО ОТКЛЮЧЕНЫ ПРЕДУПРЕЖДЕНИЯ SWAGGER

В файле проекта отключены предупреждения компилятора:

<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
    <NoWarn>$(NoWarn);8618</NoWarn>
</PropertyGroup>