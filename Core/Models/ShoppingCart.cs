﻿using System;
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

        public decimal price
        {
            get
            {
                decimal totalPrice = 0;
                if (Items is not null)
                    foreach (var item in Items)
                        totalPrice += item.price;

                return totalPrice;
            }
        }

        public virtual List<ShoppingCartItem> Items { get; set; }
    }
}
