namespace VenusPos.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio para gestionar la carga y eliminación de archivos en Azure Blob Storage
    /// </summary>
    public interface IAzureBlobStorageService
    {
        /// <summary>
        /// Sube un archivo a Azure Blob Storage
        /// </summary>
        /// <param name="stream">Stream del archivo a subir</param>
        /// <param name="nombreArchivo">Nombre único del archivo (debe incluir extensión)</param>
        /// <param name="contentType">Tipo de contenido (ej: image/jpeg, image/png)</param>
        /// <param name="contenedor">Nombre del contenedor (ej: empleados, mascotas)</param>
        /// <returns>URL pública del archivo subido</returns>
        Task<string> SubirArchivoAsync(Stream stream, string nombreArchivo, string contentType, string contenedor);

        /// <summary>
        /// Elimina un archivo de Azure Blob Storage
        /// </summary>
        /// <param name="urlArchivo">URL completa del archivo a eliminar</param>
        /// <returns>True si se eliminó correctamente, false si no se encontró</returns>
        Task<bool> EliminarArchivoAsync(string urlArchivo);

        /// <summary>
        /// Verifica si un archivo existe en Azure Blob Storage
        /// </summary>
        /// <param name="urlArchivo">URL completa del archivo</param>
        /// <returns>True si existe, false si no</returns>
        Task<bool> ExisteArchivoAsync(string urlArchivo);
    }
}
