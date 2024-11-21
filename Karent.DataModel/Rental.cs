using System;
using System.Collections.Generic;

namespace Karent.DataModel
{
    public partial class Rental
    {
        public Rental()
        {
            RentalReturns = new HashSet<RentalReturn>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int CarId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalFee { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public virtual Car Car { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<RentalReturn> RentalReturns { get; set; }
    }
}
