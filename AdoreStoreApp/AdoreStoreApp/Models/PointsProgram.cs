using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdoreStoreApp.Models
{
    public class PointsProgram
    {
        public int PointProgramId { get; set; }

        public string PointProgramName { get; set; }

        public string PointProgramDescription { get; set; }

        public int PointsAvailable { get; set; }
    }
}