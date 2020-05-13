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

        public virtual ICollection<Tweet> Tweets { get; set; }
        public virtual ICollection<TweetLike> TweetLikes { get; set; }
        public virtual ICollection<UserFollow> Followers { get; set; }
        public virtual ICollection<UserFollow> Following { get; set; }
    }
}
