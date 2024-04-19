namespace MainApp.Services
{
    // Middleware for check tokens
    public class CheckTokenMiddleware
    {
        private readonly RequestDelegate next;
        private readonly HttpClient httpClient;

        public CheckTokenMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory)
        {
            this.next = next;
            httpClient = httpClientFactory.CreateClient();
        }


        public async Task InvokeAsync(HttpContext context)
        {
            var response = await httpClient.GetAsync("/api/token/access/check");

            if (!response.IsSuccessStatusCode)
            {
                context.Response.Redirect("/login");
                return;
            }

            await next.Invoke(context);
        }
    }
}
