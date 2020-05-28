using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public virtual ICollection<Tweet> Tweets { get; set; }
        public virtual ICollection<TweetLike> TweetLikes { get; set; }
        public virtual ICollection<UserFollow> Followers { get; set; }
        public virtual ICollection<UserFollow> Following { get; set; }
        public DateTime CreatedDate { get; set; }
        public byte[] Banner { get; set; }
        public byte[] Avatar { get; set; }
        public string AvatarPath;
        public ApplicationUser() : base()
        {
            this.CreatedDate = DateTime.UtcNow;
            AvatarPath = "/assets/img/blank-profile-picture.png";
        }
    }
}
