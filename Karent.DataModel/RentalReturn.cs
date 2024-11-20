using System;
using System.Collections.Generic;

namespace Karent.DataModel
{
    public partial class RentalReturn
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal LateFee { get; set; }
        public decimal TotalFee { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public virtual Rental Rental { get; set; } = null!;
    }
}
