namespace Karent.ViewModel
{
    public class VMCar
    {
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

        // Metode validasi
        public bool IsValid(out string validationMessage)
        {
            if (string.IsNullOrWhiteSpace(Brand))
            {
                validationMessage = "Brand is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Model))
            {
                validationMessage = "Model is required.";
                return false;
            }
            if (Year <= 0)
            {
                validationMessage = "Year must be greater than 0.";
                return false;
            }

            validationMessage = string.Empty;
            return true;
        }
    }
}
