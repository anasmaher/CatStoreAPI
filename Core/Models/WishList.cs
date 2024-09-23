using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class WishList
    {
        public int Id { get; set; }

        public virtual List<Product> Products { get; set; } = new List<Product>();
    }
}
