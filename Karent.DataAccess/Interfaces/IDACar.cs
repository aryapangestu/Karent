using Karent.ViewModel;

namespace Karent.DataAccess.Interfaces
{
    public interface IDACar
    {
        VMResponse<List<VMCar>> GetByFilter(string filter);
        VMResponse<VMCar> GetById(int id);
        VMResponse<VMCar> Create(VMCar model);
        VMResponse<VMCar> Update(VMCar model);
        VMResponse<VMCar> Delete(int id);
    }
}
