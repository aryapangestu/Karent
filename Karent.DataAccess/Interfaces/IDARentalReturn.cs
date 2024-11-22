using Karent.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karent.DataAccess.Interfaces
{
    public interface IDARentalReturn
    {
        VMResponse<List<VMRentalReturn>> GetByFilter(string filter);
        VMResponse<VMRentalReturn> GetById(int id);
        VMResponse<VMRentalReturn> Create(VMRentalReturn model);
        VMResponse<VMRentalReturn> Update(VMRentalReturn model);
        VMResponse<VMRentalReturn> Delete(int id);
    }
}
