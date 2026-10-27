using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeddingAgency
{
    internal class Core
    {
        public static WeddingAgencyDB2Entities2 Context = new WeddingAgencyDB2Entities2();

        public static int CurrentUserId { get; set; }
    }
}
