using heroTopCollectionAPI.Models;
using FirebaseAdmin.Auth;
using heroTopCollectionAPI.Data.Context;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using FirebaseAdmin;

namespace heroTopCollectionAPI.Services
{
    // Clase para manejar el servicio de Firebase Authentication para perfiles que puedan acceder a endpoints sensibles
    public class AuthService
    {
        private readonly FirebaseAuth _auth;
        private readonly HttpClient _httpClient;
        private readonly string _firebaseApiKey;
        private readonly string _firebaseSignInUrl;

        public AuthService(AuthenticationContext authContext, IConfiguration configuration)
        {
            _auth = authContext.Auth;
            _httpClient = new HttpClient();
            _firebaseApiKey = configuration["Firebase:ApiKey"];
            _firebaseSignInUrl = configuration["Firebase:SignInUrl"];
        }

        public async Task<string> RegisterUserAsync(Users user, HttpContext context)
        {
            try
            {
                // Crear el usuario en Firebase
                var userRecordArgs = new UserRecordArgs
                {
                    Email = user.Email,
                    Password = user.Password,
                    EmailVerified = false,
                    Disabled = false
                };

                var userRecord = await _auth.CreateUserAsync(userRecordArgs);

                // Autenticar al usuario inmediatamente después del registro para obtener el token
                var token = await GenerateFirebaseTokenAsync(userRecord.Uid); // Generar un token de Firebase

                // Guardar el token en una cookie
                SetAuthCookie(token, context);

                return $"Usuario registrado con ID: {userRecord.Uid} y token almacenado en cookie.";
            }
            catch (Exception ex)
            {
                return $"Error al registrar usuario: {ex.Message}";
            }
        }

        // Método para iniciar sesión y devolver el token
        public async Task<string> LoginUserAsync(Users user, HttpContext context)
        {
            try
            {
                string signInUrl = $"{_firebaseSignInUrl}{_firebaseApiKey}";

                var payload = new
                {
                    email = user.Email,
                    password = user.Password,
                    returnSecureToken = true
                };

                var jsonPayload = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(signInUrl, jsonPayload);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseBody);

                if (authResponse == null || string.IsNullOrEmpty(authResponse.idToken))
                {
                    throw new InvalidOperationException("No se pudo obtener un token de autenticación válido.");
                }

                var token = authResponse.idToken;

                // Verifica si el token es válido antes de intentar establecerlo en la cookie
                if (string.IsNullOrEmpty(token))
                {
                    throw new InvalidOperationException("El token de autenticación está vacío.");
                }

                SetAuthCookie(token, context);

                return token;
            }
            catch (HttpRequestException ex)
            {
                return $"Error al iniciar sesión: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error al procesar el inicio de sesión: {ex.Message}";
            }
        }


        // Método para generar un token de Firebase para un usuario ya registrado
        private async Task<string> GenerateFirebaseTokenAsync(string userUid)
        {
            try
            {
                var customToken = await _auth.CreateCustomTokenAsync(userUid);
                return customToken;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al generar el token de Firebase: {ex.Message}");
            }
        }

        // Método para configurar la cookie con el token
        private static void SetAuthCookie(string token, HttpContext context)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,   
                SameSite = SameSiteMode.Strict, 
                Expires = DateTime.UtcNow.AddHours(1) 
            };

            context.Response.Cookies.Append("authToken", token, cookieOptions);
        }

        // Clase para deserializar la respuesta de autenticación
        private class AuthResponse
        {
            public string idToken { get; set; }
            public string LocalId { get; set; }
        }
    }
}
