# DzenCode Test - Docker Setup

## Описание

Проект полностью контейнеризован с использованием Docker и docker-compose.

## Компоненты

- **MySQL 8.0** - база данных (порт 3308)
- **Backend** - .NET 9 Web API (порт 5193)
- **Frontend** - Angular 21 + Nginx (порт 4200)

## Требования

- Docker Desktop для Windows
- Docker Compose

## Запуск проекта

### 1. Запуск всех сервисов

```powershell
docker-compose up -d --build
```

Эта команда:
- Создаст Docker образы для backend и frontend
- Запустит все контейнеры в фоновом режиме
- Автоматически применит миграции к базе данных

### 2. Проверка статуса контейнеров

```powershell
docker-compose ps
```

Все три контейнера должны быть в состоянии "Up".

### 3. Доступ к приложению

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5193/api
- **MySQL**: localhost:3308

### 4. Просмотр логов

Все сервисы:
```powershell
docker-compose logs -f
```

Конкретный сервис:
```powershell
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f mysql
```

### 5. Остановка сервисов

```powershell
docker-compose down
```

Остановка с удалением volumes (включая БД):
```powershell
docker-compose down -v
```

### 6. Пересборка конкретного сервиса

Backend:
```powershell
docker-compose up -d --build --no-deps backend
```

Frontend:
```powershell
docker-compose build --no-cache frontend
docker-compose up -d frontend
```

## Конфигурация

### База данных

- **Host**: mysql (внутри Docker сети) / localhost:3308 (снаружи)
- **Database**: DzenCodeDB
- **User**: dzenuser
- **Password**: dzenpass123
- **Root Password**: rootpassword

### Миграции

Миграции применяются автоматически при каждом запуске backend контейнера через скрипт `entrypoint.sh`.

### Volumes

Проект использует два volume для персистентности данных:
- `mysql_data` - данные MySQL
- `uploads_data` - загруженные файлы (изображения и текстовые файлы)


## Troubleshooting

### Backend не запускается

Проверьте логи:
```powershell
docker-compose logs backend
```

### Frontend показывает ошибки CORS

Убедитесь, что backend запущен и доступен:
```powershell
docker-compose ps
```

### Ошибки миграций БД

Пересоздайте volumes:
```powershell
docker-compose down -v
docker-compose up -d --build
```

### Порт уже занят

Измените порты в `docker-compose.yml`:
```yaml
ports:
  - "НОВЫЙ_ПОРТ:3306"  # для MySQL
  - "НОВЫЙ_ПОРТ:5193"  # для backend
  - "НОВЫЙ_ПОРТ:80"    # для frontend
```

## Разработка

Для локальной разработки без Docker используйте:

Backend:
```powershell
cd DzenCodeTest
dotnet run
```

Frontend:
```powershell
cd DzenCodeTest_FE/DzenCodeTest_Front
npm start
```

Не забудьте настроить connection string для локальной БД в `appsettings.Development.json`.