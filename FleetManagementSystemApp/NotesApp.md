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
___________________________________

HasForeignKey<T> указывает, в какой сущности (T) находится внешний ключ

___________________________________

Слой данных (Data Layer):
1. MyClassEFEntity;
2. Находится в MyApp.Data.Entities;
Используется для проектирования БД и взаимодействия с ней через DbContext.

Слой бизнес-логики (Business Layer):
1. MyClassDto (например, UserDto.cs);
2. Находится в MyApp.Business.DTOs или MyApp.Models.
Используется для передачи данных между сервисами и контроллерами.
Ограничивает данные, которые передаются из БД, чтобы не "светить" лишней информацией.

Слой представления (Presentation Layer):
1. MyClassViewModel (например, UserViewModel.cs);
2. Находится в MyApp.ViewModels или MyApp.Presentation.
Используется для передачи данных между контроллером и UI (Razor Pages, Views).
Может содержать дополнительные поля, специфичные для UI.

Сервисы:
Преобразуют MyClassEFEntity в MyClassDto.
Находятся в MyApp.Business.Services.

Контроллеры:
Преобразуют MyClassDto в MyClassViewModel.
Находятся в MyApp.Controllers.

Схема взаимодействия:
[БД] -> [MyClassEFEntity] -> [MyClassDto] -> [MyClassViewModel] -> [UI (Razor Pages/Views)]


# Базовый интерфейс для всех сущностей
public interface IRepository<T> : IDisposable where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(int id); // Предполагаем, что ID — это int, можно адаптировать
    Task CreateAsync(T item);
    Task UpdateAsync(T item);
    Task DeleteAsync(T item);
}

# Специфический интерфейс для пользователей
public interface IUserRepository : IRepository<User>
{
    Task<User> GetByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(User user, string password);
}

# Специфический интерфейс для сотрудников
public interface IEmployeeRepository : IRepository<Employee>
{
    Task<IEnumerable<Employee>> GetByCompanyIdAsync(int companyId);
}

# Специфический интерфейс для техники
public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<IEnumerable<Vehicle>> GetByTechnicalSpecAsync(string spec);
}

# Базовая реализация
public class Repository<T> : IRepository<T> where T : class
{
    // Реализация базовых CRUD-операций, например, через Entity Framework
    // Task<IEnumerable<T>> GetAllAsync() { ... }
    // Task<T> GetByIdAsync(int id) { ... }
    // и т.д.
}

# Реализация для пользователей
public class UserRepository : Repository<User>, IUserRepository
{
    public Task<User> GetByEmailAsync(string email)
    {
        // Логика поиска по email, возможно, с использованием Identity
    }

    public Task<bool> CheckPasswordAsync(User user, string password)
    {
        // Проверка пароля через Identity
    }
}



# Пример взаимодействия между слоями

1. 1. Слой данных (Entity):

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public UserProfile Profile { get; set; }
}

public class UserProfile
{
    public int UserProfileId { get; set; }
    public string Bio { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
}


2. Слой бизнес-логики (DTO):

public class UserDto
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Bio { get; set; } // Данные из UserProfile
}


3. Слой представления (ViewModel):

public class UserViewModel
{
    public string Name { get; set; }
    public string Bio { get; set; }
}


5. Сервис:

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public UserDto GetUserById(int id)
    {
        var user = _context.Users
            .Include(u => u.Profile)
            .FirstOrDefault(u => u.UserId == id);

        if (user == null) return null;

        return new UserDto
        {
            UserId = user.UserId,
            Name = user.Name,
            Bio = user.Profile.Bio
        };
    }
}


4. Контроллер:

public class UserController : Controller
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    public IActionResult Details(int id)
    {
        var userDto = _userService.GetUserById(id);
        var viewModel = new UserViewModel
        {
            Name = userDto.Name,
            Bio = userDto.Bio
        };
        return View(viewModel);
    }
}

[Controller] -> [Service Layer] -> [Repository]
   ↑               ↑                  ↑
  DTO           Domain Model      Domain Model

______________________________________________

# Использование коллекций

Сценарий	            Коллекция/Подход
Основная работа с БД	DbSet<T> (EF Core)
Кэширование результатов	List<T>, Dictionary<T>
Динамические запросы	IQueryable<T>
Уникальность данных	    HashSet<T>
Сырые SQL	            FromSqlRaw/Dapper
Многопоточность	        ImmutableList<T>



______________________________________________

# IMPORTANT
# Временно отключены предупреждения Swagger

В файле проекта отключены предупреждения компилятора:

<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
    <NoWarn>$(NoWarn);8618</NoWarn>
</PropertyGroup>