using Karent.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karent.DataAccess
{
    public class DARentalReturn
    {
        private readonly KarentDBContext _db;

        public DARentalReturn(KarentDBContext db)
        {
            _db = db;
        }
    }
}
