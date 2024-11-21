using Karent.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karent.ViewModel
{
    public class VMRentalReturn
    {
        public int Id { get; set; }
        public int RentalId { get; set; }


        public string UserName { get; set; } = null!;
        public string CarBrand { get; set; } = null!;
        public string CarModel { get; set; } = null!;

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

        public static VMRentalReturn FromDataModel(RentalReturn rentalReturn, Rental rental, User user, Car car)
        {
            return new VMRentalReturn
            {
                Id = rental.Id,
                RentalId = rentalReturn.RentalId,
                UserName = user.Name,
                CarBrand = car.Brand,
                CarModel = car.Model,
                ReturnDate = rentalReturn.ReturnDate,
                LateFee = rentalReturn.LateFee,
                TotalFee = rentalReturn.TotalFee,
                RentalStartDate = rental.StartDate,
                RentalEndDate = rental.EndDate,
                RentalTotalFee = rental.TotalFee,
                CreatedBy = rental.CreatedBy,
                CreatedOn = rental.CreatedOn,
                ModifiedBy = rental.ModifiedBy,
                ModifiedOn = rental.ModifiedOn
            };
        }

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
