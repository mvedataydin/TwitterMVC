using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterMVC.Models
{
    public interface ITweetRepository
    {
        IEnumerable<Tweet> GetFollowedUsersTweets(string UserId);
        IEnumerable<Tweet> GetUserTweets(string UserId);
        Tweet PostTweet(Tweet tweet);
    }
}
