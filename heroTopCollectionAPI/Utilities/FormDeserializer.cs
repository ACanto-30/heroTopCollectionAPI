using System.Text.Json;
using System.Text;

namespace heroTopCollectionAPI.Utilities
{
    public static class FormDeserializer
    {
        public static async Task ProcessFormRequest(HttpContext context)
        {
            var request = context.Request;


            if (request.ContentType != "application/x-www-form-urlencoded")
            {
                return;
            }

            try
            {
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var formBody = await reader.ReadToEndAsync();

                var keyValuePairs = formBody.Split('&')
                    .Select(pair => pair.Split('='))
                    .ToDictionary(
                        pair => Uri.UnescapeDataString(pair[0]),
                        pair => Uri.UnescapeDataString(pair[1]));

                var jsonBody = JsonSerializer.Serialize(keyValuePairs);

                var newBody = new MemoryStream(Encoding.UTF8.GetBytes(jsonBody));
                context.Request.Body = newBody;
                context.Request.ContentType = "application/json";
                newBody.Seek(0, SeekOrigin.Begin);

                if (!context.Request.Headers.ContainsKey("Authorization") && context.Request.Cookies.ContainsKey("authToken"))
                {
                    var token = context.Request.Cookies["authToken"];
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Request.Headers["Authorization"] = $"{token}";
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error processing form request: {ex.Message}");
            }
        }
    }
}
