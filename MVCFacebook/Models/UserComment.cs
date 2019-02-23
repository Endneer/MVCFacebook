using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MVCFacebook.Models
{
    public class UserComment
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("ApplicationUser")]
        public String UserID { get; set; }
        [ForeignKey("Post")]
        public int PostID { get; set; }    



        public PostState State { get; set; }
        public ApplicationUser CommentingUser { get; set; }
        public Post CommentedPost { get; set; }
        public String CommentText { get; set; }
        public DateTime CommentDate { get; set; }
    }
}
