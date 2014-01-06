using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdoreStoreApp.Models;

namespace AdoreStoreApp.ViewModels
{
    public class HomeViewModel
    {
        public MyAppUser AppUser { get; set; }

        public List<Offer> Offers { get; set; }
    }
}