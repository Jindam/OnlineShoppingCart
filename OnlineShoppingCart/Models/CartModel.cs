using OnlineShoppingCart.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShoppingCart.Models
{
    public class CartModel
    {
        public int CartId { get; set; }
        public List<ProductModel> Products { get; set; }
        public int TotalCost {
            get;

            set
            ;
        }
        public bool IsCartCheckout { get; set; }
    }
}