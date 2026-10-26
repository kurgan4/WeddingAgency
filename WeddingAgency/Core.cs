using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeddingAgency
{
    internal class Core
    {
        public static WeddingAgencyDBEntities2 Context = new WeddingAgencyDBEntities2();

        public static int CurrentUserId { get; set; }
    }
}
