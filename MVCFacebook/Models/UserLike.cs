using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MVCFacebook.Models
{
    public class UserLike
    {
        [ForeignKey("ApplicationUser")]
        public String UserID { get; set; }
        [ForeignKey("Post")]
        public int PostID { get; set; }


        public ApplicationUser LikingUser { get; set; }
        public Post LikedPost { get; set; }
    }
}
