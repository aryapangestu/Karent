namespace Karent.DataModel
{
    public partial class Car
    {
        public Car()
        {
            Rentals = new HashSet<Rental>();
        }

        public int Id { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public string PlateNumber { get; set; } = null!;
        public decimal RentalRatePerDay { get; set; }
        public decimal LateRatePerDay { get; set; }
        public string Status { get; set; } = null!;
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public virtual ICollection<Rental> Rentals { get; set; }
    }
}
