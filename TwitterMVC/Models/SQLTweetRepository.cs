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
        public void AddLikeToTweet(int tweetId, string userId)
        {
            ApplicationUser user = _context.Users.FirstOrDefault(u => u.Id == userId);
            Tweet tweet = _context.Tweets.Include(t => t.TweetLikes).Include(tl => tl.User).Single(t => t.Id == tweetId);
            if(tweet.TweetLikes.Where(tl => tl.UserId == userId).Count() == 1)
            {
                var like = tweet.TweetLikes.FirstOrDefault(tl => tl.UserId == userId);
                RemoveLikeFromTweet(like);
                return;
            }
            TweetLike tweetLike = new TweetLike()
            {
                TweetId = tweet.Id,
                UserId = userId
            };
            _context.TweetLikes.Add(tweetLike);
            _context.SaveChanges();
        }
        public void RemoveLikeFromTweet(TweetLike tweetLike)
        {
            _context.TweetLikes.Remove(tweetLike);
            _context.SaveChanges();
        }
        public Tweet PostTweet(Tweet tweet)
        {
            _context.Tweets.Add(tweet);
            _context.SaveChanges();
            return tweet;
        }
    }
}
