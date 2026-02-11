using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasess
{
    
    public class Payment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int? SubscriptionId { get; set; }

        // Navigation property to Subscription
        public Subscription? Subscription { get; set; }
    }
}
