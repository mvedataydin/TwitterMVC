using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TwitterMVC.Models;

namespace TwitterMVC.ViewModels
{
    public class IndexViewModel
    {
        public IndexViewModel()
        {
            UsersToFollow = new Collection<ApplicationUser>();
        }
        public List<Tweet> Tweets;
        public ICollection<ApplicationUser> UsersToFollow;
        public ApplicationUser CurrentUser;
    }
}
