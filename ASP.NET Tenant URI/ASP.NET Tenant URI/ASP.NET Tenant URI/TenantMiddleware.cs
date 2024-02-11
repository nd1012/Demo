using System.Net;

namespace Demo
{
    public sealed class TenantMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate Next = next;

        public Task InvokeAsync(HttpContext context)
        {
            string[] pathParts = context.Request.Path.Value?.Split('/') ?? [];
            if (pathParts.Length < 2 || pathParts[1] != "tenant" /* Validate the possible tenant alias from pathParts[1] */)
            {
                // Redirect the client to the called URI including the default tenant alias
                context.Response.StatusCode = (int)HttpStatusCode.RedirectKeepVerb;
                context.Response.Headers.Location = $"http://localhost:5113/tenant{context.Request.Path}";
                return Task.CompletedTask;
            }
            // Internal rewrite of the URI without the tenant alias (you could store the used tenant alias somewhere for later use)
            context.Request.Path = pathParts.Length > 2 ? $"/{string.Join('/', pathParts[2..])}" : "/";
            return Next(context);
        }
    }
}
