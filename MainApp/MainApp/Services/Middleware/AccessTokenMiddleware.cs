using MainApp.Models;

namespace MainApp.Services
{
    // Middleware for catch unauthorize error 401
    public class AccessTokenMiddleware
    {
        private readonly RequestDelegate next;

        public AccessTokenMiddleware(RequestDelegate next)
        {
            this.next = next;
        }


        public async Task InvokeAsync(HttpContext context, IJwtService jwtService)
        {
            // Go to request line
            await next.Invoke(context);
            // Catch 401
            if (context.Response.StatusCode == 401)
            {
                context.Response.Redirect("/login");
            }

            // Go to response line
            //await next.Invoke(context);
        }
    }
}
