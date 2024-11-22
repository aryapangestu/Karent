using Karent.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
