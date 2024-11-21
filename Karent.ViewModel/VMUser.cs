using Karent.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Karent.ViewModel
{
    public class VMUser
    {
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

        public static VMUser FromDataModel(User user)
        {
            return new VMUser
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                DrivingLicenseNumber = user.DrivingLicenseNumber,
                //Hide password
                UserType = user.UserType,
                CreatedBy = user.CreatedBy,
                CreatedOn = user.CreatedOn,
                ModifiedBy = user.ModifiedBy,
                ModifiedOn = user.ModifiedOn
            };
        }

        public bool IsValid(out string validationMessage)
        {
            validationMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Name))
            {
                validationMessage = "Name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email) || !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                validationMessage = "Valid email is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 8)
            {
                validationMessage = "Password must be at least 8 characters long.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(UserType) || !(UserType.ToLower() == "admin" || UserType.ToLower() == "customer"))
            {
                validationMessage = "UserType must be either 'admin' or 'customer'.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(PhoneNumber) && !Regex.IsMatch(PhoneNumber, @"^\+?\d{7,15}$"))
            {
                validationMessage = "PhoneNumber must be a valid phone number.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(DrivingLicenseNumber) && DrivingLicenseNumber.Length < 15)
            {
                validationMessage = "DrivingLicenseNumber must be at least 15 characters long.";
                return false;
            }

            return true;
        }
    }
}
