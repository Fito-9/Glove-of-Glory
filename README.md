# Glove-of-Glory

ANTEPROYECTO:
https://docs.google.com/document/d/1u32mrTem0lSPIUtqAcwwKoNz3m5T1maFT6zAJTRq5wo/edit?tab=t.0

ERD: 
https://drive.google.com/file/d/1ENqZcBp5f65lBeX-jiTqFNWdNpvWMd8M/view?usp=sharing

## Tecnologías usadas

- Backend: ASP.NET Core 8 + Entity Framework Core
- Base de datos: SQLite
- Frontend: Angular
- Autenticación: JWT (JSON Web Tokens)
- ORM: Entity Framework Core
- Lenguajes: C#, TypeScript, HTML, CSS

---

## Estado del proyecto (Avances actuales)

- Backend en ASP.NET Core 8 configurado como API REST.
- Base de datos SQLite integrada mediante Entity Framework.
- Implementado el sistema de usuarios:
  - Registro de usuarios con nombre, email y contraseña.
  - Subida opcional de imagen de perfil (almacenada en el servidor).
  - Contraseñas almacenadas de forma segura (hashing).
- Implementado el sistema de login seguro con JWT:
  - Se devuelve un token JWT al iniciar sesión correctamente.
  - El token permite autenticar futuras peticiones.
- Sistema para servir imágenes de perfil subidas desde el cliente.
- Configurado CORS y seguridad básica para permitir conexión entre el frontend (Angular) y el backend.

---

## Próximos pasos planeados

- Proteger endpoints con [Authorize] para requerir autenticación JWT.
- Implementar la gestión de partidas entre usuarios.
- Añadir sistema de amistades (usuarios pueden enviarse solicitudes).
- Conectar el frontend en Angular con todos los endpoints:
  - Login
  - Registro
  - Visualización y gestión de usuarios
- Añadir sistema de puntuación ELO para partidas competitivas.

---

## Instalación y ejecución del proyecto

### Requisitos previos

- .NET 8 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- Node.js: https://nodejs.org/ (para el frontend Angular)
- Angular CLI: https://angular.io/cli

### Backend (.NET Core)

```bash
# Restaurar paquetes
dotnet restore

# Ejecutar la aplicación
dotnet run