# Guía de Configuración de Azure Blob Storage

Esta guía te ayudará a configurar Azure Blob Storage para almacenar las imágenes de empleados y mascotas en la nube.

## ¿Qué se ha implementado?

Se ha integrado Azure Blob Storage en el proyecto para:
- Subir imágenes de **empleados** y **mascotas** a la nube
- Eliminar la dependencia de almacenamiento local (wwwroot/img)
- Generar URLs públicas para acceder a las imágenes desde cualquier lugar
- Preparar la aplicación para despliegue en producción

## Cambios realizados en el código

### 1. Servicios creados
- **IAzureBlobStorageService**: Interfaz del servicio (en Application/Interfaces/Services)
- **AzureBlobStorageService**: Implementación del servicio (en Infrastructure/Services)

### 2. Controladores actualizados
- **EmpleadoController**: Endpoint `/api/Empleado/upload-imagen` ahora sube a Azure
- **MascotaController**: Endpoint `/api/Mascota/upload-imagen` ahora sube a Azure

### 3. Configuración
- **Program.cs**: Registrado `IAzureBlobStorageService` en el contenedor de dependencias
- **appsettings.json**: Agregada sección `AzureBlobStorage` con placeholders

---

## Paso 1: Crear una cuenta de Azure Storage

### Opción A: Azure Portal (Recomendado para principiantes)

1. **Accede al Portal de Azure**: https://portal.azure.com
2. **Crear recurso**:
   - Click en "Crear un recurso"
   - Busca "Storage Account" o "Cuenta de almacenamiento"
   - Click en "Crear"

3. **Configuración básica**:
   - **Suscripción**: Selecciona tu suscripción (puede ser la gratuita)
   - **Grupo de recursos**: Crea uno nuevo (ej: "VenusPos-Resources")
   - **Nombre de cuenta**: Elige un nombre único (ej: "venusposimages")
     - Solo minúsculas y números
     - Entre 3 y 24 caracteres
     - Debe ser único globalmente
   - **Región**: Selecciona la más cercana a tus usuarios
   - **Rendimiento**: Standard (es suficiente para imágenes)
   - **Redundancia**: LRS (Locally Redundant Storage) - es la opción más económica

4. **Opciones avanzadas**:
   - Deja las opciones por defecto
   - Asegúrate de que "Permitir acceso público a blobs" esté habilitado

5. **Revisar y crear**:
   - Click en "Revisar y crear"
   - Click en "Crear"
   - Espera a que se complete el despliegue (1-2 minutos)

### Opción B: Azure CLI (Para usuarios avanzados)

```bash
# Iniciar sesión en Azure
az login

# Crear grupo de recursos
az group create --name VenusPos-Resources --location eastus

# Crear cuenta de almacenamiento
az storage account create \
  --name venusposimages \
  --resource-group VenusPos-Resources \
  --location eastus \
  --sku Standard_LRS \
  --allow-blob-public-access true
```

---

## Paso 2: Obtener las credenciales de acceso

1. **Ve a tu cuenta de almacenamiento** en el Portal de Azure
2. En el menú lateral, busca **"Claves de acceso"** (Access keys)
3. Verás dos claves (key1 y key2). Usa cualquiera de las dos
4. **Copia los siguientes valores**:
   - **Nombre de la cuenta de almacenamiento** (ej: venusposimages)
   - **Cadena de conexión (Connection string)** - Click en "Mostrar" y copia toda la cadena

La cadena de conexión tiene este formato:
```
DefaultEndpointsProtocol=https;AccountName=venusposimages;AccountKey=TU_CLAVE_SUPER_LARGA==;EndpointSuffix=core.windows.net
```

---

## Paso 3: Configurar appsettings.json

Abre el archivo `appsettings.json` en tu proyecto y actualiza la sección `AzureBlobStorage`:

```json
{
  "AzureBlobStorage": {
    "ConnectionString": "PEGA_AQUI_TU_CADENA_DE_CONEXION_COMPLETA",
    "AccountName": "venusposimages"
  }
}
```

### Ejemplo completo:
```json
{
  "AzureBlobStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=venusposimages;AccountKey=abcd1234EJEMPLO5678wxyz==;EndpointSuffix=core.windows.net",
    "AccountName": "venusposimages"
  }
}
```

---

## Paso 4: Verificar que todo funciona

1. **Compila el proyecto**:
   ```bash
   dotnet build
   ```

2. **Ejecuta la aplicación**:
   ```bash
   dotnet run
   ```

3. **Prueba subir una imagen**:
   - Ve a la interfaz de empleados o mascotas
   - Sube una imagen nueva
   - Verifica que se suba correctamente

4. **Verifica en Azure Portal**:
   - Ve a tu cuenta de almacenamiento
   - Click en "Contenedores" (Containers)
   - Deberías ver dos contenedores creados automáticamente:
     - `empleados`
     - `mascotas`
   - Dentro de cada contenedor verás las imágenes subidas

---

## Paso 5: Configuración de CORS (Opcional pero recomendado)

Si tu frontend está en un dominio diferente, necesitas configurar CORS en Azure:

1. Ve a tu cuenta de almacenamiento en Azure Portal
2. En el menú lateral, busca **"CORS"** (bajo Configuración)
3. En la pestaña **"Blob service"**, agrega una regla:
   - **Orígenes permitidos**: `*` (o tu dominio específico)
   - **Métodos permitidos**: GET, PUT, POST, DELETE, OPTIONS
   - **Encabezados permitidos**: `*`
   - **Encabezados expuestos**: `*`
   - **Antigüedad máxima**: 3600
4. Click en "Guardar"

---

## Estructura de URLs generadas

Cuando subes una imagen, el servicio retorna una URL con este formato:

```
https://venusposimages.blob.core.windows.net/empleados/12345678-1234-1234-1234-123456789abc.jpg
https://venusposimages.blob.core.windows.net/mascotas/98765432-9876-9876-9876-987654321xyz.png
```

Estas URLs:
- Son públicas (accesibles desde cualquier navegador)
- Se guardan en la base de datos (campo `Imagen` en empleados/mascotas)
- Funcionan desde cualquier lugar (no dependen de tu servidor local)

---

## Migración de imágenes existentes (Opcional)

Si ya tienes imágenes en `wwwroot/img/empleados` o `wwwroot/img/mascotas`, puedes migrarlas manualmente:

### Opción A: Azure Storage Explorer (Recomendado)

1. Descarga **Azure Storage Explorer**: https://azure.microsoft.com/en-us/products/storage/storage-explorer/
2. Inicia sesión con tu cuenta de Azure
3. Navega a tu cuenta de almacenamiento → Blob Containers
4. Crea los contenedores `empleados` y `mascotas` (si no existen)
5. Arrastra y suelta las imágenes desde tu carpeta local

### Opción B: Azure CLI

```bash
# Subir todas las imágenes de empleados
az storage blob upload-batch \
  --account-name venusposimages \
  --destination empleados \
  --source ./VenusPos/wwwroot/img/empleados

# Subir todas las imágenes de mascotas
az storage blob upload-batch \
  --account-name venusposimages \
  --destination mascotas \
  --source ./VenusPos/wwwroot/img/mascotas
```

Después de migrar las imágenes, necesitarás actualizar las URLs en la base de datos:

```sql
-- Actualizar URLs de empleados
UPDATE Empleados
SET Imagen = 'https://venusposimages.blob.core.windows.net/empleados/' +
             SUBSTRING(Imagen, CHARINDEX('empleados/', Imagen) + 10, LEN(Imagen))
WHERE Imagen IS NOT NULL AND Imagen LIKE '%/img/empleados/%';

-- Actualizar URLs de mascotas
UPDATE Mascotas
SET Imagen = 'https://venusposimages.blob.core.windows.net/mascotas/' +
             SUBSTRING(Imagen, CHARINDEX('mascotas/', Imagen) + 9, LEN(Imagen))
WHERE Imagen IS NOT NULL AND Imagen LIKE '%/img/mascotas/%';
```

---

## Costos de Azure Blob Storage

Azure Blob Storage es muy económico:

- **Capa gratuita**: 5 GB de almacenamiento LRS gratis durante 12 meses
- **Después de la capa gratuita**:
  - Almacenamiento: ~$0.02 USD por GB/mes
  - Operaciones: ~$0.004 USD por 10,000 operaciones de lectura
  - Transferencia de datos: Primeros 100 GB gratis al mes

**Ejemplo práctico**:
- 1,000 imágenes de 500 KB cada una = 500 MB ≈ $0.01 USD/mes
- 10,000 visualizaciones al mes ≈ $0.004 USD/mes
- **Total**: ~$0.02 USD/mes

---

## Seguridad y mejores prácticas

### 1. Usar Azure Key Vault (Producción recomendada)
En producción, **NO** guardes la ConnectionString en appsettings.json. Usa Azure Key Vault:

```bash
# Crear Key Vault
az keyvault create --name venuspos-keyvault --resource-group VenusPos-Resources --location eastus

# Guardar la ConnectionString en Key Vault
az keyvault secret set --vault-name venuspos-keyvault --name AzureStorageConnectionString --value "TU_CONNECTION_STRING"
```

### 2. Usar Managed Identity (Más seguro)
Para aplicaciones desplegadas en Azure App Service, usa Managed Identity en lugar de ConnectionString.

### 3. Configurar políticas de acceso
- Contenedores públicos solo para lectura
- Usa SAS (Shared Access Signatures) para permisos temporales
- Habilita "Soft delete" para recuperar archivos eliminados accidentalmente

### 4. Optimizar imágenes
Considera implementar:
- Compresión de imágenes antes de subir
- Generación de thumbnails automáticos
- CDN (Azure CDN) para mejorar velocidad de carga global

---

## Solución de problemas comunes

### Error: "La cadena de conexión no está configurada"
- Verifica que `appsettings.json` tenga la sección `AzureBlobStorage`
- Asegúrate de que la ConnectionString no tenga espacios extra
- Reinicia la aplicación después de modificar appsettings.json

### Error: "Access denied" o "403 Forbidden"
- Verifica que la ConnectionString sea correcta
- Asegúrate de que "Permitir acceso público a blobs" esté habilitado en Azure
- Verifica que los contenedores tengan nivel de acceso "Blob (anonymous read access for blobs only)"

### Las imágenes no se cargan en el frontend
- Verifica la URL generada (debe ser `https://...blob.core.windows.net/...`)
- Configura CORS en Azure Blob Storage
- Revisa la consola del navegador para errores específicos

### Error: "Container not found"
- Los contenedores se crean automáticamente la primera vez que subes una imagen
- Si quieres crearlos manualmente:
  ```bash
  az storage container create --name empleados --account-name venusposimages --public-access blob
  az storage container create --name mascotas --account-name venusposimages --public-access blob
  ```

---

## Próximos pasos (Opcional)

Una vez que Azure Blob Storage esté funcionando, considera:

1. **Implementar caché de imágenes** con Azure CDN
2. **Agregar resize automático** de imágenes con Azure Functions
3. **Configurar backup automático** con políticas de retención
4. **Implementar eliminación de imágenes huérfanas** (imágenes no referenciadas en la BD)
5. **Usar Azure App Service** para desplegar tu aplicación completa

---

## Recursos adicionales

- **Documentación oficial de Azure Blob Storage**: https://docs.microsoft.com/en-us/azure/storage/blobs/
- **Calculadora de precios de Azure**: https://azure.microsoft.com/en-us/pricing/calculator/
- **Azure Storage Explorer**: https://azure.microsoft.com/en-us/products/storage/storage-explorer/
- **Tutoriales de Azure Blob Storage con .NET**: https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet

---

## Soporte

Si tienes problemas con la integración:
1. Revisa los logs de la aplicación
2. Verifica las credenciales en Azure Portal
3. Consulta la sección de "Solución de problemas comunes" arriba
4. Revisa la documentación oficial de Microsoft Azure

¡Éxito con tu implementación! 🚀
