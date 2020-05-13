using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterMVC.Models
{
    public class Tweet
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<TweetLike> TweetLikes { get; set; }
        public DateTime CreatedDate { get; set; }

        public Tweet()
        {
            this.CreatedDate = DateTime.UtcNow;
        }

    }
}
