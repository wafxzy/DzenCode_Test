#!/bin/bash

echo "Starting DzenCodeTest application..."

# Ждем пока MySQL будет доступна
echo "Waiting for MySQL to be ready..."
until timeout 30 bash -c "echo >/dev/tcp/mysql/3306" 2>/dev/null; do
  echo "MySQL is unavailable - sleeping for 5 seconds"
  sleep 5
done

echo "Database connection successful!"
echo "MySQL is ready!"

# Применяем миграции
echo "Applying database migrations..."
cd /app/src/DzenCodeTest
dotnet ef database update --verbose

if [ $? -eq 0 ]; then
    echo "Migrations applied successfully!"
else
    echo "Failed to apply migrations. Exiting..."
    exit 1
fi

# Возвращаемся в директорию приложения
cd /app

# Запускаем приложение
echo "Starting the application..."
exec dotnet DzenCodeTest.dll
