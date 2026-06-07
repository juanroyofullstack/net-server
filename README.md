# net-server

> ASP.NET Core backend service with Azure Cosmos DB, optimized for AI-assisted development with GitHub Copilot.

## 🎯 Features

- **Clean Architecture** - Separation of concerns with Repository, Service, and Controller patterns
- **CosmosDB Integration** - Azure Cosmos DB for scalable, serverless data storage
- **AI-Optimized** - Comprehensive Copilot guidelines and ready-to-use prompts
- **Async/Await** - Fully asynchronous API operations
- **Error Handling** - Custom exception types and proper error responses
- **Comprehensive Testing** - Unit and integration tests with mocks
- **CI/CD Ready** - GitHub Actions workflows for quality gates

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK or later
- Azure Cosmos DB or local emulator
- Visual Studio 2022, VS Code, or Rider
- GitHub Copilot (recommended)

### Setup
```bash
# Clone repository
git clone https://github.com/juanroyofullstack/net-server.git
cd net-server

# Restore dependencies
dotnet restore

# Configure CosmosDB (create appsettings.Development.json)
# See DEVELOPMENT-SETUP.md for details

# Run application
dotnet run

# Run tests
dotnet test
```

## 📖 Documentation

- **[DEVELOPMENT-SETUP.md](DEVELOPMENT-SETUP.md)** - Complete setup guide for local development
- **[AI-DEVELOPMENT-GUIDE.md](AI-DEVELOPMENT-GUIDE.md)** - Guide for AI-assisted development with Copilot
- **[PROMPTS.md](PROMPTS.md)** - Ready-to-use Copilot prompts for common tasks
- **[.cursorrules](.cursorrules)** - AI coding guidelines and best practices
- **[.github/copilot-instructions.md](.github/copilot-instructions.md)** - Copilot-specific instructions

## 🏗️ Architecture

```
Controllers → Services → Repositories → CosmosDB
                ↓
            Logging & Error Handling
```

### Project Structure
```
src/
├── Controllers/       # REST API endpoints
├── Services/          # Business logic
├── Repositories/      # Data access
├── Models/            # Domain models & DTOs
├── Data/              # Configuration
├── Middleware/        # Custom middleware
├── Exceptions/        # Exception types
└── Extensions/        # Utility methods

tests/
├── Unit/              # Unit tests
├── Integration/       # Integration tests
└── Fixtures/          # Test data
```

## 🤖 AI Development

This project is optimized for GitHub Copilot. Get started:

1. **Read** `AI-DEVELOPMENT-GUIDE.md` for patterns and best practices
2. **Review** `.cursorrules` for coding guidelines
3. **Use** `PROMPTS.md` for ready-to-copy prompts
4. **Follow** `.github/copilot-instructions.md` for Copilot behavior

### Example Copilot Workflow
```
1. Copy prompt from PROMPTS.md
2. Paste in IDE (Copilot will suggest code)
3. Review generated code against guidelines
4. Customize as needed
5. Write tests with Copilot's help
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test class
dotnet test --filter "ServiceNameTests"
```

## 📊 Code Quality

```bash
# Format code
dotnet format

# Verify code style
dotnet format --verify-no-changes

# Build with analysis
dotnet build /p:EnforceCodeStyleInBuild=true
```

## 🔄 Development Workflow

1. **Create branch**: `git checkout -b feat/feature-name`
2. **Develop with Copilot**: Use PROMPTS.md for assistance
3. **Write tests**: Use Copilot to generate test cases
4. **Review code**: Ensure alignment with .cursorrules
5. **Commit**: `git commit -m "feat: description"`
6. **Push & PR**: Create pull request on GitHub

## 🚨 Key Guidelines

### ✅ Do's
- Use async/await for all I/O operations
- Inject dependencies via constructor
- Log all operations (info/warning/error)
- Write unit tests for public methods
- Use parameterized CosmosDB queries
- Follow SOLID principles
- Review Copilot-generated code

### ❌ Don'ts
- Don't use `.Result` or `.Wait()`
- Don't hardcode configuration values
- Don't skip error handling
- Don't ignore logging
- Don't use SQL injection patterns
- Don't ignore Copilot guideline conflicts
- Don't commit without tests

## 📚 Tech Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: Azure Cosmos DB
- **Testing**: xUnit + Moq
- **Logging**: ILogger + Serilog (recommended)
- **Language**: C# 12+
- **AI Assistant**: GitHub Copilot

## 🔧 Configuration

### Environment Variables
```json
{
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=...;AccountKey=...",
    "DatabaseName": "net-server",
    "ContainerName": "items"
  }
}
```

### Dependency Injection
All services are registered in `Program.cs`:
```csharp
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

## 📈 Performance

- Async operations throughout
- CosmosDB query optimization
- Caching layer support
- Pagination for large datasets
- Minimal memory footprint

## 🔐 Security

- Input validation on all endpoints
- Parameterized queries for CosmosDB
- Exception details not exposed to clients
- HTTPS enforced in production
- Configuration via secure services (Key Vault)

## 🤝 Contributing

1. Read `DEVELOPMENT-SETUP.md`
2. Follow `.cursorrules` guidelines
3. Use `PROMPTS.md` for Copilot assistance
4. Write tests for new features
5. Submit PR with description

## 📝 License

MIT License - see LICENSE file for details

## 🆘 Support

- **Documentation**: Check the docs folder
- **Issues**: Report on GitHub Issues
- **Discussions**: Use GitHub Discussions
- **Development Help**: See `DEVELOPMENT-SETUP.md`
- **Copilot Help**: See `AI-DEVELOPMENT-GUIDE.md`

## 🎯 Roadmap

- [ ] Add authentication/authorization
- [ ] Implement caching layer
- [ ] Add API rate limiting
- [ ] Create admin dashboard
- [ ] Add WebSocket support
- [ ] Implement background jobs

---

**Last Updated**: 2026-06-07  
**Maintained by**: @juanroyofullstack  
**Status**: 🚀 Active Development

Start developing with AI assistance! See `AI-DEVELOPMENT-GUIDE.md` →
