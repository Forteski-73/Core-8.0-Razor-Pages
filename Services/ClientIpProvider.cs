namespace OXF.Services
{
    public class ClientIpProvider : IClientIpProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientIpProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetClientIp()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return "unknown";

            string ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (!string.IsNullOrEmpty(ip))
                ip = ip.Split(',').First().Trim();
            else
                ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return ip;
        }
    }
}
