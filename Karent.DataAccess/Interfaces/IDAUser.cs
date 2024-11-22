using Karent.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karent.DataAccess.Interfaces
{
    public interface IDAUser
    {
        VMResponse<List<VMUser>> GetByFilter(string filter);
        VMResponse<VMUser> GetById(int id);
        VMResponse<VMUser> Create(VMUser model);
        VMResponse<VMUser> Update(VMUser model);
        VMResponse<VMUser> Delete(int id);
        VMResponse<VMUser> Login(string email, string password);
    }
}
