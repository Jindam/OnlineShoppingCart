using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShoppingCart.Models.Product
{
    public interface IProductListHandler
    {
        IEnumerable<ProductModel> Handle(ProductListQuery query);
    }
}
