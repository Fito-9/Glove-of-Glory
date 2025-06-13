# Glove-of-Glory

PRIMER VIDEO:
https://drive.google.com/file/d/1NSiuy5zYzOuvrhN8ZuEaex4FBrkz9KX6/view?usp=sharing

ANTEPROYECTO:
https://docs.google.com/document/d/1u32mrTem0lSPIUtqAcwwKoNz3m5T1maFT6zAJTRq5wo/edit?tab=t.0

## Autores
*   **Adolfo Burgos Belgrano**
*   **Gonzalo Ruiz Azuar**

---

## 1. Descripción del Proyecto

**Glove of Glory** es un ecosistema de aplicaciones diseñado para la comunidad competitiva del videojuego *Super Smash Bros. Ultimate*. El proyecto se estructura en dos experiencias complementarias: una **aplicación web principal** para la competición y la interacción social, y una **aplicación móvil de bolsillo** para la consulta rápida de información.

Esta arquitectura dual, soportada por **dos backends independientes**, permite ofrecer una experiencia optimizada para cada plataforma:

*   La **aplicación web** es el corazón de la plataforma, donde los jugadores pueden encontrar partidas, competir en un sistema de ranking ELO, gestionar amigos y participar en salas de juego en tiempo real.
*   La **aplicación móvil para Android** actúa como, una guía de referencia rápida para que los jugadores, tanto nuevos como veteranos, puedan consultar información esencial sobre personajes y escenarios en cualquier momento y lugar, facilitando así su aprendizaje y mejora estratégica.

---

## 2. Objetivos del Proyecto

*   **Crear una Plataforma de Competición Web:** Desarrollar una aplicación web robusta con matchmaking, sistema de ELO, gestión de amigos y salas de juego en tiempo real mediante WebSockets.
*   **Desarrollar una App Móvil:** Construir una aplicación nativa para Android que sirva como guía de consulta rápida, ofreciendo información detallada sobre personajes y escenarios para facilitar el aprendizaje del juego.
*   **Arquitectura de Micro-servicios (Backend Dual):**
    *   Implementar un **backend principal (.NET)** para la aplicación web, que gestione toda la lógica compleja de matchmaking, amistades, ranking y comunicación en tiempo real.
    *   Implementar un **backend secundario y ligero (.NET)** exclusivamente para la aplicación móvil, gestionando la autenticación y el servicio de datos de manera optimizada y segura para dispositivos móviles.
*   **Experiencia de Usuario Diferenciada:** Ofrecer una interfaz y funcionalidades específicas para cada plataforma, reconociendo que las necesidades de un usuario en su PC (competir) son diferentes a las de su móvil (consultar).
*   **Administración Centralizada:** Dotar a la plataforma web de un panel de administración para la gestión de usuarios.

---
## 3. Tecnologías Utilizadas

El proyecto está dividido en cuatro partes principales:

### Frontend (Aplicación Web)
*   **Framework:** Angular 
*   **Lenguaje:** TypeScript
*   **Estilos:** CSS con variables
*   **Comunicación:** Cliente HTTP para API REST y WebSockets para comunicación en tiempo real.

### Backend (API Central para la Web)
*   **Framework:** ASP.NET Core 8
*   **Lenguaje:** C#
*   **Base de Datos:** SQLite con Entity Framework Core (Code-First).
*   **Autenticación:** JWT (JSON Web Tokens).
*   **Comunicación en tiempo real:** WebSockets.
*   **Arquitectura:** API RESTful para operaciones CRUD y un gestor de WebSockets para la lógica de juego.

### Cliente Móvil y su Backend
*   **Aplicación Android:**
    *   **Lenguaje:** Kotlin
    *   **UI Framework:** Jetpack Compose para una interfaz declarativa y moderna.
    *   **Arquitectura:** MVVM (Model-View-ViewModel) con StateFlow para la gestión de estado.
    *   **Comunicación:** Retrofit para llamadas a la API REST.
    *   **Asíncronía:** Coroutines de Kotlin.
*   **Backend para Android:**
    *   **Framework:** ASP.NET Core 8
    *   **Lenguaje:** C#
    *   **Base de Datos:** SQLite con Entity Framework Core.
    *   **Autenticación:** Sistema de sesión simple basado en tokens.

---
## 4. Esquema de la Base de Datos (Entidad-Relación)

La base de datos se ha diseñado utilizando un enfoque "Code-First" con Entity Framework Core. Las entidades principales y sus relaciones son:

*   **User:** Almacena la información del usuario (ID, nickname, email, hash de contraseña, ELO, rol y ruta del avatar).
*   **Friendship:** Tabla intermedia que representa una relación de amistad entre dos usuarios. Contiene el ID del emisor, el ID del receptor y un booleano `IsAccepted` para gestionar las solicitudes pendientes.
*   **Match:** Registra el resultado de cada partida, incluyendo los IDs de los jugadores, el ID del ganador, los personajes utilizados y el mapa.

A continuación se muestra el diagrama Entidad-Relación:

![image](https://github.com/user-attachments/assets/0bc6a5d1-50b1-41fe-87c0-81487f459e78)

---

## 6. Despliegue

*   **URL de la Aplicación Web:** `[Pega aquí la URL donde esté desplegada tu app de Angular]`
*   **URL del Backend:** `[Pega aquí la URL donde esté desplegada tu API de .NET]`

---

## 7. Diseño de la Aplicación (Prototipo)

El diseño de la interfaz y la experiencia de usuario se ha prototipado utilizando Uizard.

*   **URL del Prototipo:** [https://app.uizard.io/prototypes/qyYGgMWbQjsye6ZejxJG/player/overview]
    **Powerby**![image](https://github.com/user-attachments/assets/64e1b63c-150b-4624-8ec7-69dba2635144)

---

## 8. Presentación y Vídeo ----- ## 5. Tutorial de Uso

*   **Presentación PDF:** `[Sube tu PDF a Google Drive o similar y pega aquí el enlace]`
*   **Vídeo Demostrativo (5 min):** `[Sube tu vídeo a YouTube/Vimeo y pega aquí el enlace]`


---
## 9. Bitácora de Tareas

---

## 10. Bibliografía y Recursos

Para el desarrollo de este proyecto, además de la documentación oficial de las tecnologías utilizadas, se ha recurrido a las siguientes fuentes de información y comunidades para asegurar la fidelidad a la temática de *Super Smash Bros. Ultimate*:

*   **Super Smash Bros. Ultimate - Sitio Oficial:** Fuente principal para obtener los recursos gráficos oficiales de personajes y escenarios, así como información básica del juego.
    *   [https://www.smashbros.com/en_US/](https://www.smashbros.com/en_US/)
*   **SmashWiki (SSBWiki):** La enciclopedia comunitaria más completa sobre la saga Smash. Esencial para consultar datos técnicos, *frame data*, y detalles sobre las mecánicas de juego que inspiraron las funcionalidades de la aplicación.
    *   [https://www.ssbwiki.com/](https://www.ssbwiki.com/)
*   **Documentación Oficial de Angular:** [https://angular.dev/](https://angular.dev/)
*   **Documentación Oficial de ASP.NET Core:** [https://docs.microsoft.com/aspnet/core/](https://docs.microsoft.com/aspnet/core/)
*   **Documentación Oficial de Android Developers (Kotlin & Compose):** [https://developer.android.com/](https://developer.android.com/)



