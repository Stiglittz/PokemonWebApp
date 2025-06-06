@echo off
echo 🚀 Configurando repositorio Git para PokemonWebApp...
echo.

REM Verificar si Git está instalado
git --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Git no está instalado. Por favor instala Git primero.
    pause
    exit /b 1
)

REM Inicializar repositorio Git
echo 📁 Inicializando repositorio Git...
git init
if errorlevel 1 (
    echo ❌ Error inicializando Git
    pause
    exit /b 1
)

REM Configurar usuario Git (opcional)
echo.
echo 👤 Configurando usuario Git...
set /p git_name="Introduce tu nombre para Git (ej: Juan Pérez): "
set /p git_email="Introduce tu email para Git (ej: juan@email.com): "

git config user.name "%git_name%"
git config user.email "%git_email%"

REM Agregar archivos al staging area
echo.
echo 📋 Agregando archivos al repositorio...
git add .gitignore
git add .editorconfig
git add README.md
git add TECHNICAL-DOCS.md
git add appsettings.json
git add appsettings.Development.json
git add appsettings.Production.json

REM Agregar archivos del proyecto
git add Program.cs
git add PokemonWebApp.csproj
git add PokemonWebApp.sln

REM Agregar carpetas del proyecto
git add Controllers/
git add Models/
git add Services/
git add Views/
git add wwwroot/
git add Properties/
git add Middleware/

echo.
echo 📝 Creando commits organizados...

REM Commit inicial con documentación
git commit -m "📚 docs: Add initial project documentation

- Add comprehensive README.md with features and setup
- Add TECHNICAL-DOCS.md with architecture details
- Add .gitignore for .NET Core projects
- Add .editorconfig for code consistency
- Configure appsettings for different environments"

if errorlevel 1 (
    echo ❌ Error en el commit inicial
    pause
    exit /b 1
)

REM Commit de configuración del proyecto
git commit -m "⚙️ config: Initial project setup and configuration

- Configure ASP.NET Core 8 MVC project
- Setup dependency injection container
- Configure Serilog logging with file rotation
- Add StatusCodePages and error handling
- Configure HttpClient for PokeAPI consumption"

REM Commit de modelos y servicios
git commit -m "🏗️ feat: Add core models and services architecture

- Add Pokemon domain models (Pokemon, PokemonType, PokemonSprites)
- Add PokemonSpecies and PokemonAbility models
- Add ViewModels for Index and Details views
- Implement IPokemonService interface
- Add IExcelService and IEmailService interfaces"

REM Commit de implementación de servicios
git commit -m "⚡ feat: Implement core services with caching

- Implement PokemonService with HttpClient and IMemoryCache
- Add intelligent caching system (server + client side)
- Implement ExcelService with ClosedXML
- Implement EmailService with SMTP configuration
- Add comprehensive error handling and logging"

REM Commit de controladores
git commit -m "🎮 feat: Add controllers and routing

- Implement PokemonController with all CRUD operations
- Add AJAX endpoints for Pokemon details
- Add Excel export functionality (individual/bulk)
- Add email sending functionality (individual/bulk)
- Implement ErrorController with custom error pages"

REM Commit de vistas y UI
git commit -m "🎨 feat: Create responsive UI with advanced features

- Add responsive Pokemon grid with Bootstrap 5
- Implement advanced filtering (name, type, height)
- Add pagination with visual indicators
- Create Pokemon details modal with AJAX
- Add bulk selection with checkboxes
- Implement type-specific color schemes"

REM Commit de JavaScript y interactividad
git commit -m "⚡ feat: Add client-side functionality and caching

- Implement pokemon-cache.js for localStorage caching
- Add modal interactions and AJAX calls
- Add bulk operations JavaScript
- Implement lazy loading for images
- Add responsive design enhancements
- Add cache debugging tools"

REM Commit de middleware y error handling
git commit -m "🛡️ feat: Add comprehensive error handling and logging

- Implement GlobalExceptionHandlingMiddleware
- Add custom error pages with friendly messages
- Configure Serilog with structured logging
- Add request/response logging with metrics
- Implement cache statistics and monitoring"

echo.
echo ✅ Repositorio Git configurado exitosamente!
echo.
echo 📊 Estado del repositorio:
git status --porcelain
echo.
echo 📈 Historial de commits:
git log --oneline --graph
echo.
echo 🔗 Para conectar con GitHub:
echo    1. Crea un repositorio en GitHub
echo    2. Ejecuta: git remote add origin https://github.com/tu-usuario/PokemonWebApp.git
echo    3. Ejecuta: git branch -M main
echo    4. Ejecuta: git push -u origin main
echo.
pause