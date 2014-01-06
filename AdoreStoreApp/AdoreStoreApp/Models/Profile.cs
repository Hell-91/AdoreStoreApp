using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdoreStoreApp.Models
{
    public class Profile
    {
        public int ProfileId { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string EmailId { get; set; }

        public string Address { get; set; }

        public string PhoneNo { get; set; }

        public string AuthToken { get; set; }
    }
}