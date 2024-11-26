using Karent.ViewModel;

namespace Karent.DataAccess.Interfaces
{
    public interface IDARentalReturn
    {
        VMResponse<List<VMRentalReturn>> GetByFilter(string filter);
        VMResponse<List<VMRentalReturn>> GetByFilter(string filter, int userId);
        VMResponse<VMRentalReturn> GetById(int id);
        VMResponse<VMRentalReturn> GetById(int id, int userId);
        VMResponse<VMRentalReturn> Create(VMRentalReturn model);
        VMResponse<VMRentalReturn> Update(VMRentalReturn model);
        VMResponse<VMRentalReturn> Delete(int id);
    }
}
