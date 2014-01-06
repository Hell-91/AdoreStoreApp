using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdoreStoreApp.Models
{
    public class Offer
    {
        public int OfferId { get; set; }

        public string OfferName { get; set; }

        public string OfferDescription { get; set; }

        public DateTime OfferStartDate { get; set; }

        public DateTime OfferEndDate { get; set; }

        public string OfferGraphic { get; set; }

        public bool OfferOpted { get; set; }

        public bool AllowOfferToBeShared { get; set; }

        public bool OfferSharedCount { get; set; }
    }
}