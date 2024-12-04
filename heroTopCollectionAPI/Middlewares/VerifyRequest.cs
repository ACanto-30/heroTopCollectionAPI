using FirebaseAdmin.Auth;
using Google.Protobuf.WellKnownTypes;
using heroTopCollectionAPI.Attributes;
using heroTopCollectionAPI.Data.Context;
using heroTopCollectionAPI.Services;
using heroTopCollectionAPI.Utilities;

namespace heroTopCollectionAPI.Middlewares
{
    public class VerifyRequest
    {
        private readonly RequestDelegate _next;
        private readonly FirebaseAuth _auth;
        private readonly UsersService _usersService;

        public VerifyRequest(RequestDelegate next, AuthenticationContext authContext, UsersService usersService)
        {
            _auth = authContext.Auth;
            _next = next;
            _usersService = usersService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            var headers = request.Headers;
            var endpoint = request.Path.Value;

            if (request.ContentType == "application/x-www-form-urlencoded")
            {
                await FormDeserializer.ProcessFormRequest(context);
            }

            // Obtener la anotación EndpointRuleAttribute del endpoint actual
            var endpointMetadata = context.GetEndpoint()?.Metadata.GetMetadata<EndpointRuleAttribute>();
            if (endpointMetadata == null)
            {
                // Si no hay anotación, permitir la solicitud
                await _next(context);
                return;
            }

            // Verificar que el Content-Type de la solicitud coincida con el esperado
            var expectedContentType = endpointMetadata.ContentType;
            var actualContentType = headers["Content-Type"].ToString();
            if (!actualContentType.Contains(expectedContentType))
            {
                context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                await context.Response.WriteAsync($"Invalid Content-Type. Expected: {expectedContentType}, Received: {actualContentType}");
                return;
            }

            // Verificar si el endpoint requiere autorización
            if (endpointMetadata.RequiresAuthorization)
            {
                string? token = null;

                // Determinar la fuente del token según el Content-Type
                if (actualContentType.Contains("application/json"))
                {
                    // Token en los headers
                    if (!headers.ContainsKey("Authorization"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Authorization header is missing.");
                        return;
                    }
                    else
                    {
                        token = headers["Authorization"];
                    }
                }
                else if (context.Request.Cookies.ContainsKey("authToken"))
                {
                    // Token en las cookies
                    token = context.Request.Cookies["authToken"];
                }
                else
                {
                    token = headers["Authorization"];
                }

                if (string.IsNullOrEmpty(token) || !await ValidateTokenAsync(token))
                {
                    context.Response.Redirect("/Home");
                    return;
                }
            }

            // Continuar con la solicitud si pasa las verificaciones
            await _next(context);
        }


        private async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                await _auth.VerifyIdTokenAsync(token);
                return true;
            }
            catch
            {
                if (await _usersService.IsTokenValid(token))
                    return true;
                else
                    return false;
            }
        }
    }
}
