using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karent.ViewModel
{
    public class VMLogin
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        public bool IsValid(out string validationMessage)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                validationMessage = "Email is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                validationMessage = "Password is required.";
                return false;
            }

            validationMessage = string.Empty;
            return true;
        }
    }
}
