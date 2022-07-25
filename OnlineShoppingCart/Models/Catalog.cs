using OnlineShoppingCart.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShoppingCart.Models
{
    public class CatalogModel
    {
        public int CatalogId { get; set; }
        public List<ProductModel> Products { get; set; }
    }
}