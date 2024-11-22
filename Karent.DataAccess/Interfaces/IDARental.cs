using Karent.ViewModel;

namespace Karent.DataAccess.Interfaces
{
    public interface IDARental
    {
        VMResponse<List<VMRental>> GetByFilter(string filter);
        VMResponse<VMRental> GetById(int id);
        VMResponse<VMRental> Create(VMRental model);
        VMResponse<VMRental> Update(VMRental model);
        VMResponse<VMRental> Delete(int id);
    }
}
