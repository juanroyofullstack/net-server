# GitHub Copilot Instructions for net-server

## Project Overview
This is an ASP.NET Core backend service with Azure CosmosDB for data persistence.

## Code Generation Guidelines

### 1. Service Layer Generation
When asked to create a new service, Copilot should generate:
```csharp
public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
}

public class UserService : IUserService
{
    private readonly IRepository<User> _repository;
    private readonly ILogger<UserService> _logger;

    public UserService(IRepository<User> repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UserDto> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching user with ID: {UserId}", userId);
        var user = await _repository.GetAsync(userId, userId);
        return user != null ? MapToDto(user) : throw new NotFoundException($"User {userId} not found");
    }
}
```

### 2. Repository Pattern for CosmosDB
Always include:
- Partition key handling
- Query parameter passing
- Proper async/await
- Error logging

```csharp
public class Repository<T> : IRepository<T> where T : class
{
    private readonly Container _container;
    private readonly ILogger<Repository<T>> _logger;

    public async Task<T> GetAsync(string id, string partitionKey)
    {
        _logger.LogDebug("Getting {EntityType} with ID: {Id}", typeof(T).Name, id);
        return await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
    }

    public async Task<IEnumerable<T>> QueryAsync(string query, Dictionary<string, object> parameters)
    {
        var queryDef = new QueryDefinition(query);
        foreach (var param in parameters)
        {
            queryDef = queryDef.WithParameter(param.Key, param.Value);
        }
        var iterator = _container.GetItemQueryIterator<T>(queryDef);
        var results = new List<T>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }
}
```

### 3. Controller Generation
Include:
- Proper route attributes
- Request/response DTOs
- Model validation
- Error responses
- Logging

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(string id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/v1/users/{Id}", id);
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/v1/users");
        var user = await _userService.CreateUserAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }
}
```

### 4. Unit Test Generation
Always generate with:
- AAA pattern
- Mocked dependencies
- Multiple test cases

```csharp
public class UserServiceTests
{
    private readonly Mock<IRepository<User>> _mockRepository;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IRepository<User>>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _service = new UserService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = "user-123";
        var user = new User { Id = userId, Name = "John" };
        _mockRepository.Setup(r => r.GetAsync(userId, userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        _mockRepository.Verify(r => r.GetAsync(userId, userId), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        var userId = "invalid-id";
        _mockRepository.Setup(r => r.GetAsync(userId, userId))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetUserByIdAsync(userId)
        );
    }
}
```

## CosmosDB-Specific Prompts

### Query Generation
When generating CosmosDB queries, use parameterized queries:
```
Generate a query to find all active users created in the last 30 days
```

Should produce:
```csharp
var query = @"SELECT * FROM c WHERE c.type = @type AND c.isActive = true AND c.createdAt >= @startDate";
var parameters = new Dictionary<string, object>
{
    { "@type", "User" },
    { "@startDate", DateTime.UtcNow.AddDays(-30) }
};
var users = await _repository.QueryAsync(query, parameters);
```

### Performance Optimization
Common optimization prompts:
- "Optimize this query for CosmosDB" → Reduce RU consumption
- "Add pagination to this endpoint" → Handle large datasets
- "Implement caching for this data" → Reduce unnecessary queries

## Configuration & Setup

### Environment Variables
Keep in `appsettings.json`:
```json
{
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://...",
    "DatabaseName": "net-server",
    "ContainerName": "users"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Dependency Injection Setup
In `Program.cs`:
```csharp
// CosmosDB
builder.Services.AddSingleton<CosmosClient>(sp =>
    new CosmosClient(builder.Configuration["CosmosDb:ConnectionString"]));

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Services
builder.Services.AddScoped<IUserService, UserService>();
```

## Common Patterns

### Async/Await
✅ Always use async patterns:
```csharp
public async Task<T> GetAsync(string id)
{
    return await _repository.GetAsync(id);
}
```

❌ Never use `.Result` or `.Wait()`:
```csharp
// DON'T DO THIS
var user = _repository.GetAsync(id).Result;
```

### Error Handling
Custom exceptions:
```csharp
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
```

### Logging
Always log operations:
```csharp
_logger.LogInformation("User {UserId} created successfully", user.Id);
_logger.LogWarning("Failed to update user {UserId}", userId);
_logger.LogError("Database error: {Message}", ex.Message);
```

## Prompts to Avoid

❌ **Don't ask Copilot to:**
- Generate code without error handling
- Create hardcoded values
- Skip logging
- Use synchronous database calls
- Ignore partition keys in CosmosDB

✅ **Instead ask:**
- "Create a service method to..." (with full context)
- "Generate tests for..." (will include mocks)
- "Optimize this query for CosmosDB" (will consider RU)
- "Add error handling to..." (will use custom exceptions)

## References
- [Cursor Rules](../.cursorrules)
- [ASP.NET Core Best Practices](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules)
- [CosmosDB Documentation](https://docs.microsoft.com/en-us/azure/cosmos-db/)
