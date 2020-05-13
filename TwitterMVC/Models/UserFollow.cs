using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterMVC.Models
{
    public class UserFollow
    {

        public ApplicationUser User { get; set; }
        public string UserId { get; set; }
        public ApplicationUser Follower { get; set; }
        public string FollowerId { get; set; }
    }
}
