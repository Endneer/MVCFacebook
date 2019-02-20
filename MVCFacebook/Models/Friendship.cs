using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MVCFacebook.Models
{
    public class Friendship
    {

        public String User1ID { get; set; }
        public String User2ID { get; set; }


        public ApplicationUser User1 { get; set; }
        public ApplicationUser User2 { get; set; }
        public bool Pending { get; set; }
    }
}
