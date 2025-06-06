# 🎮 PokemonWebApp - Aplicación Web con .NET Core 8 MVC

Una aplicación web moderna desarrollada con **ASP.NET Core 8 MVC** que consume la [PokéAPI](https://pokeapi.co/) para mostrar información detallada de Pokémon con funcionalidades avanzadas de filtrado, exportación y comunicación por email.

![.NET Core](https://img.shields.io/badge/.NET%20Core-8.0-purple)
![C#](https://img.shields.io/badge/C%23-11.0-blue)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.0-blueviolet)
![License](https://img.shields.io/badge/License-MIT-green)

## 🌟 Características Principales

### ✨ Funcionalidades Core
- **🔍 Búsqueda y Filtrado Avanzado**: Filtros por nombre, tipo y altura con múltiples criterios
- **📄 Paginación Inteligente**: Navegación eficiente con indicadores visuales
- **🎨 Interfaz Moderna**: Design responsivo con colores específicos por tipo de Pokémon
- **⚡ Cache Inteligente**: Sistema dual de cache (servidor + cliente) para máximo rendimiento
- **📊 Exportación a Excel**: Exportación individual, masiva o de selección personalizada
- **📧 Sistema de Emails**: Envío individual o masivo con templates HTML profesionales

### 🚀 Funcionalidades Avanzadas
- **🔎 Modal de Detalles**: Vista completa con estadísticas, habilidades y especies
- **🌈 Colores Dinámicos**: UI que se adapta al tipo principal del Pokémon
- **📈 Barras de Progreso**: Visualización animada de estadísticas
- **🏆 Badges Especiales**: Identificación de Pokémon legendarios y míticos
- **🔄 Selección Masiva**: Checkboxes para operaciones en lote
- **📱 Diseño Responsivo**: Optimizado para dispositivos móviles y desktop

### 🛠️ Características Técnicas
- **📝 Logging Profesional**: Serilog con archivos rotativos y logging estructurado
- **🛡️ Manejo de Errores**: Middleware global con páginas de error personalizadas
- **⚡ Llamadas Asíncronas**: HttpClient optimizado con manejo robusto de errores
- **💾 Cache Multinivel**: IMemoryCache en servidor + localStorage en cliente
- **🔧 Configuración por Entorno**: Configuraciones separadas para Development/Production

## 📷 Screenshots

### Vista Principal
![Vista Principal](https://via.placeholder.com/800x400/4f46e5/white?text=Pokemon+Grid+View)

### Modal de Detalles
![Modal de Detalles](https://via.placeholder.com/800x400/059669/white?text=Pokemon+Details+Modal)

### Filtros Avanzados
![Filtros](https://via.placeholder.com/800x400/dc2626/white?text=Advanced+Filters)

## 🚀 Instalación y Configuración

### Prerrequisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)
- Conexión a Internet (para consumir PokéAPI)

### 1. Clonar el Repositorio
```bash
git clone https://github.com/tu-usuario/PokemonWebApp.git
cd PokemonWebApp
```

### 2. Restaurar Dependencias
```bash
dotnet restore
```

### 3. Configurar Email (Opcional)
Edita `appsettings.Development.json` para configurar el envío de emails:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "tu-email@gmail.com",
    "SmtpPassword": "tu-contraseña-de-aplicacion",
    "EnableSsl": true,
    "FromEmail": "tu-email@gmail.com",
    "FromName": "PokemonWebApp"
  }
}
```

> **💡 Tip para Gmail**: Usa [contraseñas de aplicación](https://support.google.com/mail/answer/185833) en lugar de tu contraseña regular.

### 4. Ejecutar la Aplicación
```bash
dotnet run
```

La aplicación estará disponible en: `https://localhost:5001` o `http://localhost:5000`

## 📚 Arquitectura del Proyecto

### Estructura de Carpetas
```
PokemonWebApp/
├── Controllers/           # Controladores MVC
│   ├── PokemonController.cs
│   ├── ErrorController.cs
│   └── HomeController.cs
├── Models/               # Modelos de datos
│   ├── Pokemon/         # Modelos de Pokémon
│   └── ViewModels/      # ViewModels para vistas
├── Services/            # Servicios de negocio
│   ├── PokemonService.cs    # Consumo de PokéAPI
│   ├── ExcelService.cs      # Exportación Excel
│   └── EmailService.cs      # Envío de emails
├── Views/               # Vistas Razor
│   ├── Pokemon/         # Vistas de Pokémon
│   ├── Error/           # Páginas de error
│   └── Shared/          # Layouts compartidos
├── wwwroot/             # Archivos estáticos
│   ├── css/            # Estilos CSS
│   ├── js/             # JavaScript
│   └── lib/            # Librerías cliente
├── Middleware/          # Middleware personalizado
└── Logs/               # Archivos de log
```

### Tecnologías Utilizadas

#### Backend
- **ASP.NET Core 8 MVC** - Framework web
- **HttpClient** - Consumo de APIs REST
- **IMemoryCache** - Cache en memoria
- **Serilog** - Logging estructurado
- **ClosedXML** - Generación de archivos Excel
- **System.Net.Mail** - Envío de emails SMTP

#### Frontend
- **Bootstrap 5** - Framework CSS
- **Font Awesome** - Iconografía
- **JavaScript ES6+** - Interactividad
- **localStorage API** - Cache del cliente
- **Fetch API** - Llamadas AJAX

#### APIs Externas
- **[PokéAPI](https://pokeapi.co/)** - Datos de Pokémon
- **[PokéAPI Species](https://pokeapi.co/docs/v2#pokemon-species)** - Información de especies

## 🔧 Configuración Avanzada

### Variables de Entorno para Producción
```bash
# Email Configuration
SMTP_SERVER=smtp.example.com
SMTP_PORT=587
SMTP_USERNAME=your-email
SMTP_PASSWORD=your-password
FROM_EMAIL=noreply@yourapp.com

# Application Insights (opcional)
APPINSIGHTS_CONNECTION_STRING=your-connection-string

# Logging Level
ASPNETCORE_ENVIRONMENT=Production
```

### Cache Configuration
El sistema de cache puede configurarse en `appsettings.json`:

```json
{
  "CacheSettings": {
    "DefaultTtlMinutes": 60,
    "PokemonDetailsTtlMinutes": 120,
    "PokemonTypesTtlMinutes": 360
  }
}
```

### Logging Configuration
Configuración personalizable de Serilog:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

## 📊 Rendimiento y Optimizaciones

### Métricas de Cache
- **70-80% menos llamadas** a la API para datos previamente visitados
- **Cache de tipos**: 6 horas de duración
- **Cache de detalles**: 30 minutos de duración
- **Cache de búsquedas**: 15 minutos de duración

### Optimizaciones Implementadas
- ✅ Precarga automática de Pokémon populares
- ✅ Lazy loading de imágenes
- ✅ Paginación eficiente
- ✅ Compresión automática de respuestas
- ✅ Minificación de CSS/JS en producción
- ✅ CDN para librerías externas

## 🧪 Testing

### Ejecutar Tests (Cuando estén disponibles)
```bash
dotnet test
```

### Testing Manual
1. **Filtros**: Prueba todas las combinaciones de filtros
2. **Paginación**: Navega entre páginas y verifica datos
3. **Modal de detalles**: Abre múltiples Pokémon y verifica información
4. **Exportación**: Descarga Excel con diferentes selecciones
5. **Cache**: Verifica velocidad en segunda visita
6. **Responsive**: Prueba en diferentes dispositivos

## 🐛 Debugging y Troubleshooting

### Logs Comunes
```bash
# Ver logs en tiempo real
tail -f Logs/pokemon-app-$(date +%Y%m%d).log

# Verificar errores de API
grep "ERROR" Logs/pokemon-app-*.log
```

### Problemas Frecuentes

#### 1. Error de conexión a PokéAPI
```
🔍 Verificar conectividad: ping pokeapi.co
✅ Revisar logs de HttpClient
⚡ Verificar configuración de timeout
```

#### 2. Problemas de Email
```
📧 Verificar configuración SMTP
🔐 Confirmar contraseña de aplicación
🌐 Verificar puertos de firewall (587, 465)
```

#### 3. Cache no funciona
```
💾 Verificar configuración de IMemoryCache
🔧 Limpiar localStorage del navegador
📊 Revisar logs de cache hits/misses
```

## 🤝 Contribución

### Proceso de Contribución
1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

### Estándares de Código
- Seguir convenciones de C# y .NET
- Documentar métodos públicos con XML comments
- Mantener coverage de tests > 80%
- Usar logging apropiado con Serilog
- Seguir principios SOLID

## 📝 Roadmap

### Próximas Funcionalidades
- [ ] 🧪 **Testing unitario y de integración**
- [ ] 🔐 **Autenticación y autorización**
- [ ] 🌍 **Internacionalización (i18n)**
- [ ] 📱 **PWA (Progressive Web App)**
- [ ] 🎮 **Modo batalla entre Pokémon**
- [ ] 📈 **Dashboard de administración**
- [ ] 🔄 **Sincronización offline**
- [ ] 🎨 **Temas personalizables**

### Mejoras Técnicas
- [ ] 🏗️ **Migración a .NET 9**
- [ ] 🐳 **Containerización con Docker**
- [ ] ☁️ **Deploy en Azure/AWS**
- [ ] 📊 **Métricas y monitoreo**
- [ ] 🔄 **CI/CD pipeline**
- [ ] 🎯 **Performance profiling**

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo [LICENSE](LICENSE) para más detalles.

## 👨‍💻 Autor

**Hugo GSaenz**
- GitHub: [@Stiglittz](https://github.com/Stiglittz)
- LinkedIn: [hgogzzsaenz](https://www.linkedin.com/in/hgogzzsaenz/)
- Email: hgogzzs@gmail.com

## 🙏 Agradecimientos

- [PokéAPI](https://pokeapi.co/) por proporcionar datos gratuitos de Pokémon
- [Bootstrap](https://getbootstrap.com/) por el framework CSS
- [Font Awesome](https://fontawesome.com/) por los iconos
- [Serilog](https://serilog.net/) por el sistema de logging
- [ClosedXML](https://github.com/ClosedXML/ClosedXML) por la generación de Excel

---

## 📊 Estadísticas del Proyecto

![GitHub Stats](https://img.shields.io/github/languages/top/tu-usuario/PokemonWebApp)
![GitHub Size](https://img.shields.io/github/repo-size/tu-usuario/PokemonWebApp)
![GitHub Commits](https://img.shields.io/github/commit-activity/m/tu-usuario/PokemonWebApp)

**⭐ Si este proyecto te resultó útil, no olvides darle una estrella en GitHub ⭐**