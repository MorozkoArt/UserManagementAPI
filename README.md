# User Management API

![.NET Version](https://img.shields.io/badge/.NET-9.0-blue)
![Swagger Support](https://img.shields.io/badge/Swagger-UI-green)

## 📝 Описание

RESTful Web API для управления пользователями с полным набором CRUD-операций, реализованный на ASP.NET Core 9. Сервис включает:

- Аутентификацию и авторизацию
- Ролевую модель (Администратор/Пользователь)
- Мягкое удаление записей
- Валидацию входных данных
- Полную документацию через Swagger UI

## Описание проекта

### Структура проекта

```plaintext
📂UserManagement/
├──📂Controllers/                  # Логика приложения
├──📂Exceptions/                   # Генерация файлов
├──📂Models/                       # Модели данных
├──📂Services/                     # Парсер входных данных
├──📂Utilities/                    # Вспомогательные функции
├──📄appsettings.Development.json  # Вспомогательные функции
├──📄appsettings.json              # Вспомогательные функции
├──📄UserManagement.http           # Вспомогательные функции
└──📄Program.cs                    # Вспомогательные функции
```
