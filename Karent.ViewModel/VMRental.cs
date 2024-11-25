namespace Karent.ViewModel
{
    public class VMRental
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int CarId { get; set; }
        public string? CarBrand { get; set; }
        public string? CarModel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalFee { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public bool IsValid(out string validationMessage)
        {
            validationMessage = string.Empty;

            // Validasi UserId
            if (UserId <= 0)
            {
                validationMessage = "User ID must be greater than zero.";
                return false;
            }

            // Validasi CarId
            if (CarId <= 0)
            {
                validationMessage = "Car ID must be greater than zero.";
                return false;
            }

            // Validasi StartDate dan EndDate
            if (StartDate == default || EndDate == default)
            {
                validationMessage = "Start date and end date are required.";
                return false;
            }
            if (StartDate > EndDate)
            {
                validationMessage = "Start date cannot be later than end date.";
                return false;
            }

            // Validasi TotalFee
            if (TotalFee <= 0)
            {
                validationMessage = "Total fee must be greater than zero.";
                return false;
            }

            // Jika semua validasi lolos
            return true;
        }
    }
}
