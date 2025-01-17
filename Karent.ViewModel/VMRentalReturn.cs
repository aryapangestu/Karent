﻿namespace Karent.ViewModel
{
    public class VMRentalReturn
    {
        public int Id { get; set; }
        public int RentalId { get; set; }


        public string? UserName { get; set; }
        public string? CarBrand { get; set; }
        public string? CarModel { get; set; }

        public DateTime ReturnDate { get; set; }
        public decimal LateFee { get; set; }
        public decimal TotalFee { get; set; }

        public DateTime RentalStartDate { get; set; }
        public DateTime RentalEndDate { get; set; }
        public decimal RentalTotalFee { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public bool IsValid(out string validationMessage)
        {
            validationMessage = string.Empty;

            if (RentalId <= 0)
            {
                validationMessage = "Rental ID must be greater than zero.";
                return false;
            }

            if (ReturnDate == default)
            {
                validationMessage = "Return date is required.";
                return false;
            }

            if (LateFee < 0)
            {
                validationMessage = "Late fee cannot be negative.";
                return false;
            }

            if (TotalFee <= 0)
            {
                validationMessage = "Total fee must be greater than zero.";
                return false;
            }

            return true;
        }
    }
}
