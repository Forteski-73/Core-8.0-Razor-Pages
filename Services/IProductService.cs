namespace OXF.Services
{
    public interface IProductService
    {
        Task<(bool Success, string? ErrorMessage)> ImportAsync(IEnumerable<object> produtos, string jwtToken);
    }
}
