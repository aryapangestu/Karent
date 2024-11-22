using Karent.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
