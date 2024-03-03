using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace restaurantBot
{
    public class ReservationInfo
    {
        public int IdReservation { get; set; }
        public int IdTable { get; set; }
        public string RegDate { get; set; }
        public string ReserveDate { get; set; }
        public int idClient { get; set; }
        public string ReserveTime { get; set; }
        public string CountPeople { get; set; }
        public string Confirmation { get; set; }

    }
}
