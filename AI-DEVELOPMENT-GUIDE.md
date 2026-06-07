# AI-Optimized Development Guide for net-server

## Overview
This guide provides best practices and patterns for developing with GitHub Copilot in the net-server project.

## 📁 Project Structure

```
net-server/
├── src/
│   ├── Controllers/          # API endpoints - REST handlers
│   ├── Services/             # Business logic layer
│   ├── Repositories/         # CosmosDB data access layer
│   ├── Models/
│   │   ├── Domain/           # Core business entities
│   │   ├── Dto/              # Data Transfer Objects
│   │   └── Requests/         # API request models
│   ├── Data/                 # CosmosDB configuration & seeding
│   ├── Middleware/           # Custom middleware
│   ├── Exceptions/           # Custom exception types
│   ├── Extensions/           # Extension methods & utilities
│   └── Program.cs            # DI configuration & app setup
├── tests/
│   ├── Unit/                 # Unit tests
│   ├── Integration/          # Integration tests
│   └── Fixtures/             # Test data & helpers
├── .cursorrules              # AI coding guidelines
└── .github/copilot-instructions.md
```

## 🤖 Working with Copilot

### Prompt Templates

#### 1. Creating a New Feature
```
Create a new feature for [feature name] with:
- Service interface (IService)
- Service implementation with logging and error handling
- Repository pattern for CosmosDB operations
- Controller with CRUD endpoints
- Unit tests with mocks
- DTOs for request/response

Use partition key: [partition_key_field]
Include error handling for common scenarios
```

#### 2. Optimizing a Query
```
Optimize this CosmosDB query for lower RU consumption:
[paste current query]

Current partition key strategy: [explain]
Expected result set size: [size]
Access pattern: [how it's typically used]
```

#### 3. Adding Tests
```
Generate comprehensive unit tests for [ServiceName]:
- Test successful operations
- Test error scenarios
- Test edge cases
- Mock all dependencies
- Use AAA pattern
```

#### 4. Refactoring Code
```
Refactor this method to follow clean architecture:
[paste code]

Requirements:
- Use dependency injection
- Add proper logging
- Include error handling
- Add XML documentation
```

### Effective Prompt Writing

✅ **Good Prompts:**
- Include context about what you're building
- Specify the pattern you want to follow
- Mention edge cases or special requirements
- Reference existing code patterns in the project

❌ **Avoid:**
- Generic requests like "write a service"
- Missing context about CosmosDB usage
- No mention of error handling needs
- Requests without partition key details

## 📝 Common Development Tasks

### Adding a New Entity

1. **Create Domain Model:**
```csharp
public class Product
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    public string Name { get; set; }
    
    [JsonProperty("_partitionKey")]
    public string PartitionKey { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public string Type => "Product"; // Discriminator
}
```

2. **Create DTOs:**
```csharp
public class ProductDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProductRequest
{
    public string Name { get; set; }
}
```

3. **Create Repository Interface:**
```csharp
public interface IProductRepository
{
    Task<ProductDto> GetByIdAsync(string id, string partitionKey);
    Task<IEnumerable<ProductDto>> GetAllAsync(string partitionKey);
    Task<ProductDto> CreateAsync(Product product);
    Task<ProductDto> UpdateAsync(Product product);
    Task DeleteAsync(string id, string partitionKey);
}
```

4. **Use Copilot:**
```
Create a ProductService that implements IProductService with:
- Dependency injection of IProductRepository and ILogger
- Methods: GetById, GetAll, Create, Update, Delete
- Full error handling and logging
- DTO mapping from domain models
- Validation of input

Partition key strategy: use productId
```

### Testing Strategy

**Unit Test Structure:**
```csharp
public class ServiceNameTests
{
    // Setup fixtures
    private readonly Mock<IDependency> _mockDependency;
    private readonly ServiceName _service;

    // Test constructor - initialize mocks
    
    // Happy path tests
    [Fact]
    public async Task MethodName_SuccessCondition_ReturnsExpected() { }
    
    // Error scenario tests
    [Fact]
    public async Task MethodName_ErrorCondition_ThrowsException() { }
    
    // Edge case tests
    [Fact]
    public async Task MethodName_EdgeCase_HandlesCorrectly() { }
}
```

### CosmosDB Query Patterns

**Parameterized Query (Preferred):**
```csharp
var query = @"
    SELECT * FROM c 
    WHERE c.type = @type 
    AND c.status = @status 
    AND c.createdAt >= @startDate
    ORDER BY c.createdAt DESC";

var parameters = new Dictionary<string, object>
{
    { "@type", "Product" },
    { "@status", "Active" },
    { "@startDate", DateTime.UtcNow.AddDays(-30) }
};

var products = await _repository.QueryAsync(query, parameters);
```

**Pagination Pattern:**
```csharp
public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public string ContinuationToken { get; set; }
    public int Count { get; set; }
}

public async Task<PaginatedResult<T>> QueryAsync(
    string query, 
    Dictionary<string, object> parameters,
    string continuationToken = null,
    int pageSize = 100)
{
    // Implementation using iterator and continuation token
}
```

## 🔍 Code Review Checklist for AI-Generated Code

When Copilot generates code, verify:

- [ ] Error handling is present (try-catch or proper validation)
- [ ] Logging statements for debugging and monitoring
- [ ] Async/await used for all I/O operations
- [ ] CosmosDB partition keys handled correctly
- [ ] Dependency injection through constructor
- [ ] XML documentation for public methods
- [ ] No hardcoded values (use configuration)
- [ ] Unit tests with proper mocks
- [ ] SOLID principles followed
- [ ] No N+1 query problems

## 🚀 Performance Optimization Tips

### CosmosDB RU Optimization
1. **Select only needed fields:**
```csharp
SELECT c.id, c.name FROM c WHERE c.type = @type
```

2. **Use appropriate indexes**
3. **Batch operations when possible**
4. **Implement pagination for large result sets**

### Caching Strategy
```csharp
public class CachedProductRepository : IProductRepository
{
    private readonly IProductRepository _innerRepository;
    private readonly IMemoryCache _cache;

    public async Task<ProductDto> GetByIdAsync(string id, string partitionKey)
    {
        var cacheKey = $"product-{id}";
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _innerRepository.GetByIdAsync(id, partitionKey);
            }
        );
    }
}
```

## 📚 Useful Copilot Prompts Library

### Architecture
- "Create a clean architecture structure for [domain]"
- "Show me the repository pattern implementation for CosmosDB"
- "Generate service layer with dependency injection"

### Features
- "Generate CRUD operations for [entity] with error handling"
- "Create an async API endpoint that handles pagination"
- "Generate a query service with filtering, sorting, and pagination"

### Testing
- "Generate comprehensive unit tests with mocks and edge cases"
- "Create integration tests for CosmosDB repository"
- "Generate test fixtures for [entity]"

### Optimization
- "Optimize this CosmosDB query to reduce RU consumption"
- "Add caching layer for frequently accessed data"
- "Generate pagination implementation for large datasets"

### Documentation
- "Generate XML documentation for this interface"
- "Create API documentation with examples"
- "Generate architecture decision records (ADRs)"

## 🔧 Development Workflow

1. **Plan the feature** - Define entities, endpoints, logic
2. **Use Copilot to scaffold** - Generate service/repo/controller
3. **Review generated code** - Check checklist above
4. **Write/adjust tests** - Use Copilot to generate test cases
5. **Optimize** - Ask Copilot for performance improvements
6. **Document** - Use Copilot to generate documentation

## 🎯 Best Practices

1. **Always specify context** - The more details, the better Copilot's suggestions
2. **Review first** - Don't blindly accept generated code
3. **Ask for patterns** - "Use the repository pattern" gets better results
4. **Test thoroughly** - Generated code needs verification
5. **Iterate** - Copilot's first suggestion isn't always perfect
6. **Learn from it** - Use it to understand best practices

## 📖 References

- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [CosmosDB Best Practices](https://learn.microsoft.com/en-us/azure/cosmos-db/best-practices)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Async/Await Best Practices](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)

## 🆘 Getting Help

When stuck:
1. Check the `.cursorrules` file for guidelines
2. Review `.github/copilot-instructions.md` for patterns
3. Look at existing code in the project
4. Ask Copilot with full context and requirements
5. Verify the generated code matches project standards

---

Happy coding with AI! 🚀
