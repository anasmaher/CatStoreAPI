using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }

        public virtual List<ShoppingCartItem> Items { get; set; }
    }
}
