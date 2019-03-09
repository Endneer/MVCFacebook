using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MVCFacebook.Models.UserViewModels
{
    public class SettingsViewModel
    {
        [Required]        
        public string Old_Password { get; set; }

        [Required]
        public string New_Password { get; set; }

        [Required]
        public string Confirm_Password { get; set; }

    }
}
