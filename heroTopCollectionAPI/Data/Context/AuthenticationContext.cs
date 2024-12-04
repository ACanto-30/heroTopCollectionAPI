using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace heroTopCollectionAPI.Data.Context
{
    public class AuthenticationContext
    {
        public FirebaseAuth Auth { get; }

        public AuthenticationContext(IConfiguration configuration)
        {
            var firebaseCredentialsPath = configuration["Firebase:CredentialsPath"];
            if (string.IsNullOrEmpty(firebaseCredentialsPath))
            {
                throw new InvalidOperationException("La ruta de credenciales de Firebase no está configurada.");
            }

            // Inicializar FirebaseApp
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(firebaseCredentialsPath)
            });

            // Inicializar FirebaseAuth
            Auth = FirebaseAuth.DefaultInstance;
        }
    }
}
