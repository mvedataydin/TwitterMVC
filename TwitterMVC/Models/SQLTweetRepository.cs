using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterMVC.Data;

namespace TwitterMVC.Models
{
    public class SQLTweetRepository : ITweetRepository
    {
        private readonly AppDbContext _context;
        public SQLTweetRepository(AppDbContext context)
        {
            _context = context;
        }
        public IEnumerable<Tweet> GetFollowedUsersTweets(string UserId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Tweet> GetUserTweets(string UserId)
        {
            return _context.Tweets.Where(tw => tw.User.Id == UserId);
        }

        public Tweet PostTweet(Tweet tweet)
        {
            _context.Tweets.Add(tweet);
            _context.SaveChanges();
            return tweet;
        }
    }
}
