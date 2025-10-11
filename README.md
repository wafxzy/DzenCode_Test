# DzenCode Test - Docker Setup

## NOTE: AFTER DOCKER STARTS SUCCESSFULLY, WAIT AT LEAST 5 MINUTES FOR THE BACKEND BUILD
## Or verify the backend build logs with:
```
Start-Sleep -Seconds 15; docker-compose logs backend | Select-Object -Last 30
```

## Description

This project is fully containerized using Docker and docker-compose.

## Components

- **MySQL 8.0** - database (host port 3308)
- **Backend** - .NET 9 Web API (port 5193)
- **Frontend** - Angular 21 + Nginx (port 4200)

## Requirements

- Docker Desktop for Windows
- Docker Compose

## Running the project

### 1. Start all services

```powershell
docker-compose up -d --build
```

This command:
- Builds Docker images for the backend and frontend
- Starts all containers in the background
- Automatically applies database migrations

### 2. Check container status

```powershell
docker-compose ps
```

All three containers should show `Up`.

### 3. Access the application

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5193/api
- **MySQL**: localhost:3308

### 4. View logs

All services:
```powershell
docker-compose logs -f
```

Single service:
```powershell
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f mysql
```

### 5. Stop services

```powershell
docker-compose down
```

Stop and remove volumes (this will delete the DB data):
```powershell
docker-compose down -v
```

### 6. Rebuild a specific service

Backend:
```powershell
docker-compose up -d --build --no-deps backend
```

Frontend:
```powershell
docker-compose build --no-cache frontend
docker-compose up -d frontend
```

## Configuration

### Database

- **Host**: `mysql` (inside Docker network) / `localhost:3308` (host)
- **Database**: `DzenCodeDB`
- **User**: `dzenuser`
- **Password**: `dzenpass123`
- **Root Password**: `rootpassword`

### Migrations

EF Core migrations are applied automatically on each backend container start via the `entrypoint.sh` script.

### Volumes

The project uses two Docker volumes for data persistence:
- `mysql_data` - MySQL data
- `uploads_data` - uploaded files (images and text files)


## Troubleshooting

### Backend won't start

Check backend logs:
```powershell
docker-compose logs backend
```

### Frontend shows CORS errors

Make sure backend is running and accessible:
```powershell
docker-compose ps
```

### Database migration errors

Recreate volumes and restart:
```powershell
docker-compose down -v
docker-compose up -d --build
```

### Port already in use

Change ports in `docker-compose.yml` (example):
```yaml
ports:
  - "NEW_PORT:3306"  # MySQL
  - "NEW_PORT:5193"  # backend
  - "NEW_PORT:80"    # frontend
```

## Development (without Docker)

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

Don't forget to adjust the connection string for your local database in `appsettings.Development.json`.
