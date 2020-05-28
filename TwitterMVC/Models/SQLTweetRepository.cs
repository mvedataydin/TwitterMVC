using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterMVC.Data;
using Microsoft.EntityFrameworkCore;
namespace TwitterMVC.Models
{
    public class SQLTweetRepository : ITweetRepository
    {
        private readonly AppDbContext _context;
        public SQLTweetRepository(AppDbContext context)
        {
            _context = context;
        }
        public IEnumerable<Tweet> GetFollowedUsersTweets(List<string> followedUsers)
        {
            return _context.Tweets.Include(t => t.TweetLikes).Where(u => followedUsers.Contains(u.User.Id));
        }
        public IEnumerable<Tweet> GetUserTweets(string UserId)
        {
            return _context.Tweets.Include(t => t.TweetLikes).Where(tw => tw.User.Id == UserId);
        }
        public TweetLike AddLikeToTweet(int tweetId)
        {
            Tweet tweet = _context.Tweets.FirstOrDefault(tw => tw.Id == tweetId);
            TweetLike tweetLike = new TweetLike()
            {
                TweetId = tweet.Id,
                UserId = tweet.User.Id
            };
            _context.TweetLikes.Add(tweetLike);
            _context.SaveChanges();
            return tweetLike;
        }

        public Tweet PostTweet(Tweet tweet)
        {
            _context.Tweets.Add(tweet);
            _context.SaveChanges();
            return tweet;
        }
    }
}
