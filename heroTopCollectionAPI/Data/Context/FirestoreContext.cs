using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;

namespace heroTopCollectionAPI.Data.Context
{
    public class FirestoreContext
    {
        // Propiedad pública para acceder a FirestoreDb
        public FirestoreDb FirestoreDb { get; set; }

        public FirestoreContext(IConfiguration configuration)
        {
            var firebaseCredentialsPath = configuration["Firebase:CredentialsPath"];

            if (string.IsNullOrEmpty(firebaseCredentialsPath))
            {
                throw new InvalidOperationException("La ruta de credenciales de Firebase no está configurada.");
            }

            // Inicializa Firestore con las credenciales
            var credential = GoogleCredential.FromFile(firebaseCredentialsPath);
            FirestoreClient client = new FirestoreClientBuilder { Credential = credential }.Build();
            FirestoreDb = FirestoreDb.Create(configuration["Firebase:ProjectId"], client);

        }
    }
}
