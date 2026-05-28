#!/bin/bash
set -e

echo "Inicializando SQL Server..."

# Esperar a que SQL Server esté listo
sleep 15s

echo "SQL Server está listo, creando base de datos..."

# Conectar y crear la base de datos si no existe
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD <<EOF
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SupermercadoDB')
BEGIN
    CREATE DATABASE SupermercadoDB
END
GO

USE SupermercadoDB
GO

-- Crear usuario adicional (opcional)
IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'apiuser')
BEGIN
    CREATE LOGIN apiuser WITH PASSWORD = 'ApiUser123!@'
END
GO

USE SupermercadoDB
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'apiuser')
BEGIN
    CREATE USER apiuser FOR LOGIN apiuser
    EXEC sp_addrolemember 'db_owner', 'apiuser'
END
GO

PRINT 'Base de datos SupermercadoDB creada/verificada correctamente'
EOF

echo "Inicialización completada"
