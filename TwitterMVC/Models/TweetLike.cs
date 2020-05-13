using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterMVC.Models
{
    public class TweetLike
    {

        public int TweetId { get; set; }
        public Tweet Tweet { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
