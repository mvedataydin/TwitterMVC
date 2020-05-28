using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TwitterMVC.Models;

namespace TwitterMVC.ViewModels
{
    public class ProfileViewModel
    {
        public ProfileViewModel()
        {
            UsersToFollow = new Collection<ApplicationUser>();
        }
        public List<Tweet> Tweets;
        public ICollection<ApplicationUser> UsersToFollow;
        public ApplicationUser CurrentUser;
        public IFormFile Avatar;
        public IFormFile Banner;
    }
}
