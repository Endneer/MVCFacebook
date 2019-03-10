using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MVCFacebook.Models.UserViewModels
{
    public class EditInfoViewModel
    {
        [Required]
        public String FirstName { get; set; }

        [Required]
        public String Lastname { get; set; }

        [Required]
        public String Biography { get; set; }

    }
}
