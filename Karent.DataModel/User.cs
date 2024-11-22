namespace Karent.DataModel
{
    public partial class User
    {
        public User()
        {
            Rentals = new HashSet<Rental>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DrivingLicenseNumber { get; set; }
        public string Password { get; set; } = null!;
        public string UserType { get; set; } = null!;
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public virtual ICollection<Rental> Rentals { get; set; }
    }
}
