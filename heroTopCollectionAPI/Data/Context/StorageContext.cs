using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using System;

namespace heroTopCollectionAPI.Data.Context
{
    public class StorageContext
    {
        // Propiedad pública para acceder a StorageClient
        public StorageClient Client { get; }

        public StorageContext(IConfiguration configuration)
        {
            // Obtiene la ruta de credenciales desde la configuración
            var firebaseCredentialsPath = configuration["Firebase:CredentialsPath"];

            if (string.IsNullOrEmpty(firebaseCredentialsPath))
            {
                throw new InvalidOperationException("La ruta de credenciales de Firebase no está configurada.");
            }

            // Carga las credenciales desde el archivo JSON
            var credential = GoogleCredential.FromFile(firebaseCredentialsPath);
            Client = StorageClient.Create(credential);
        }
    }
}
