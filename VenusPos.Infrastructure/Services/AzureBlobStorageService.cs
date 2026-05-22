using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using VenusPos.Application.Interfaces.Services;

namespace VenusPos.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de Azure Blob Storage
    /// </summary>
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _accountName;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlobStorage:ConnectionString"]
                ?? throw new InvalidOperationException("La cadena de conexión de Azure Blob Storage no está configurada");

            _accountName = configuration["AzureBlobStorage:AccountName"]
                ?? throw new InvalidOperationException("El nombre de la cuenta de Azure Storage no está configurado");

            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> SubirArchivoAsync(Stream stream, string nombreArchivo, string contentType, string contenedor)
        {
            try
            {
                // Normalizar nombre del contenedor (debe ser lowercase)
                contenedor = contenedor.ToLowerInvariant();

                // Obtener o crear el contenedor
                var containerClient = _blobServiceClient.GetBlobContainerClient(contenedor);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                // Obtener referencia al blob
                var blobClient = containerClient.GetBlobClient(nombreArchivo);

                // Configurar encabezados HTTP
                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                };

                // Subir el archivo
                stream.Position = 0; // Asegurar que el stream esté al inicio
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });

                // Retornar la URL pública del blob
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al subir archivo a Azure Blob Storage: {ex.Message}", ex);
            }
        }

        public async Task<bool> EliminarArchivoAsync(string urlArchivo)
        {
            try
            {
                // Extraer contenedor y nombre del archivo de la URL
                var (contenedor, nombreArchivo) = ExtraerInfoDeUrl(urlArchivo);

                if (string.IsNullOrEmpty(contenedor) || string.IsNullOrEmpty(nombreArchivo))
                    return false;

                var containerClient = _blobServiceClient.GetBlobContainerClient(contenedor);
                var blobClient = containerClient.GetBlobClient(nombreArchivo);

                return await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExisteArchivoAsync(string urlArchivo)
        {
            try
            {
                // Extraer contenedor y nombre del archivo de la URL
                var (contenedor, nombreArchivo) = ExtraerInfoDeUrl(urlArchivo);

                if (string.IsNullOrEmpty(contenedor) || string.IsNullOrEmpty(nombreArchivo))
                    return false;

                var containerClient = _blobServiceClient.GetBlobContainerClient(contenedor);
                var blobClient = containerClient.GetBlobClient(nombreArchivo);

                return await blobClient.ExistsAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Extrae el nombre del contenedor y del archivo desde una URL de Azure Blob
        /// Ejemplo: https://cuentastorage.blob.core.windows.net/empleados/imagen.jpg
        /// Retorna: (empleados, imagen.jpg)
        /// </summary>
        private (string contenedor, string nombreArchivo) ExtraerInfoDeUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.TrimStart('/').Split('/', 2);

                if (segments.Length < 2)
                    return (string.Empty, string.Empty);

                return (segments[0], segments[1]);
            }
            catch
            {
                return (string.Empty, string.Empty);
            }
        }
    }
}
