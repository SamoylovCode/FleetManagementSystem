# Docker
Запуск двух docker-compose: docker-compose и docker-compose.dev
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d

Опция --force-recreate в команде docker-compose up заставляет Docker Compose пересоздать контейнеры, даже если их конфигурация или образ не изменился
docker compose -f docker-compose.yml -f docker-compose.dev.yml up -d --force-recreate app

Аналогично для остановки
docker-compose -f docker-compose.yml -f docker-compose.dev.yml down

Сборка конкретно приложения
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up app -d

Флаг -v удаляет именованные тома, определённые в compose файлах 

Запуск без кэша
docker-compose build --no-cache

Пересобрать образ:
docker-compose up -d --build

Запуск только Redis:
docker-compose up -d redis

Удаление всех контейнеров
docker system prune -a

Удаление всех томов
docker volume prune

Удаление конкретного контейнера
docker rm <контейнер>

Просмотр переменных окружения
docker exec -it fleet-app sh -c "printenv"

Вход в контейнер
docker exec -it <контейнер> sh

Вывод списка файлов, которые содержатся в контейнере (после входа в контейнер)
ls -la

Просмотр содержимого файла, который содержится в контейнере
cat <название_файла>

# Redis
PM> PACKAGE MANAGE CONSOLE:
docker ps /* вывод всех запущенных контейнеров с названием redis (<redis-name>) */
docker exec -it <redis-name> redis-cli /* вход в интерфейс командной строки Redis */
FLUSHDB /* очистка текущей БД Redis */
FLUSHALL /* очистка всех баз Redis */
exit /* выход из интерфейса командной строки Redis */

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

- Get-Migration - получение списка всех миграций и их статусов.
- Update-Database -Migration <Предыдущая_миграция> - откатить последнюю примененную миграцию
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


# FORM

class="form-control-plaintext" readonly

# Универсальная схема применения префиксов в кэшировании

Уровень           Пример ключа                            Когда использовать                        Метод из CachePrefixes
----------------- --------------------------------------- ----------------------------------------- ------------------------------------
Подмодель         691d388b...:VehicleIdentificationData   Кэш подмодели                             VehicleAggregateSubModelKey
                  691d388b...:Insurance                   (VehicleIdentificationData,               (vehicleId, subModelKey)
                                                          Insurance, Passport...)
Полный агрегат    vehicle:aggregate:691d388b...:full      Кэш агрегата VehicleDataDto,              VehicleAggregateFull(vehicleId)
                                                          VehiclePageViewModel...
Глобальные списки vehicles:list                           Кэш списков транспортных средств          Константа VehiclesList
Другие сущности   user:by-id:123e4567...                  Выборка пользователей по ID/email         Константы UserById, UserByEmail
                  user:by-email:user@...


# Очистить кэш NuGet
dotnet nuget locals all --clear
dotnet restore
dotnet build