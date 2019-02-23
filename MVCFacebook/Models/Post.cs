using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MVCFacebook.Models
{
    public class Post
    {
        public String Text { get; set; }
        public DateTime CreationDate { get; set; }
        public ApplicationUser Creator { get; set; }
        public PostState State { get; set; }

        [Key]
        public int ID { get; set; }

        public ICollection<UserComment> Comments { get; set; }
        public ICollection<UserLike> Likes { get; set; }
    }
}
