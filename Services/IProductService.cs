namespace OXF.Services
{
    public interface IProductService
    {
        Task<(bool Success, string? ErrorMessage)> ImportProductAsync(IEnumerable<object> produtos, string jwtToken);

        Task<(bool Success, string? ErrorMessage)> ImportInventAsync(IEnumerable<object> produtos, string jwtToken);

        Task<(bool Success, string? ErrorMessage)> ImportOxfordAsync(IEnumerable<object> produtos, string jwtToken);

        Task<(bool Success, string? ErrorMessage)> ImportTaxInformationAsync(IEnumerable<object> produtos, string jwtToken);

    }
}
