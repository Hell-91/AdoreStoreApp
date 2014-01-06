using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdoreStoreApp.Models;

namespace AdoreStoreApp.ViewModels
{
    public class ProfileViewModel
    {
        public Profile Profile { get; set; }

        public List<PointsProgram> Points { get; set; }
    }
}