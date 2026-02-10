using OXF.Models;

namespace OXF.Services
{
    public interface IInventoryService
    {
        Task<List<Inventory>> GetAllAsync(string jwtToken);
        Task<List<InventoryRecord>?> GetRecordsByGuidAsync(string code, string jwtToken);
    }
}