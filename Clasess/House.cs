using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Clasess
{
    public class House
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }

        public ICollection<Subscription>? Subscriptions { get; set; }
    }
}
