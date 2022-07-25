using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OnlineShoppingCart.Helpers;
using OnlineShoppingCart.Models;
using OnlineShoppingCart.Models.Product;
//using ShoppingCart.Api.Models;
//using ShoppingCart.Api.Models.Product;
//using ShoppingCart.Products;

namespace OnlineShoppingCart
{

    [Route("api")]
    public class CartController : Controller
    {
        private const string cartItemCacheKey = "cartItemCache";
        private readonly IProductListHandler _handler;
        private readonly IMapper _mapper;
        private IMemoryCache _cache;

        public CartController(IProductListHandler handler, IMemoryCache cache, IMapper mapper)
        {
            _handler = handler;
            _mapper = mapper;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            refreshCache(JsonFileReader.ReadByItem<CartModel>(@".\Data\Cart.json"));

            //if (_cache.TryGetValue(cartItemCacheKey, out CartModel _cartItem))
            //{
            //    if(_cartItem != null)
            //        refreshCache(JsonFileReader.ReadByItem<CartModel>(@".\Data\Cart.json"));
            //}
               
        }

        // GET: api/Product
        [HttpGet]
        [Route("cart")]
        public IActionResult Get()
        {
            try
            {
                var data = JsonFileReader.ReadByItem<CartModel>(@".\Data\Cart.json");

                if (_cache.TryGetValue(cartItemCacheKey, out CartModel _cartItem) && _cartItem != null)
                    return Ok(new ServiceResponseVM() { Success = true, Data = _cartItem });

                return Ok(new ServiceResponseVM() { Success = true, Data = "No elements in cart" });
            }
            catch (FileNotFoundException exception)
            {
                return StatusCode(501, new ServiceResponseVM() { Success = false, Data = exception });
            }
        }

        [HttpGet]
        [Route("cart/item/{id:int}")]
        public IActionResult GetbyId(int id)
        {
            ProductModel product = null;

            if (_cache.TryGetValue(cartItemCacheKey, out CartModel cartItem) && cartItem != null)
                product = cartItem.Products.FirstOrDefault(x => x.ID == id);

            if (product != null)
                return Ok(new ServiceResponseVM() { Success = true, Data = product });
            else
                return StatusCode(404, new ServiceResponseVM() { Success = false });
        }

        // find cartId and add item to products
        [HttpPost]
        [Route("cart/item/{id:int}")]
        public IActionResult AddItem(int id, [FromBody] ProductModel productVm)
        {
            if (_cache.TryGetValue(cartItemCacheKey, out CartModel _cartItem) && _cartItem != null)
            {
                var productExists = _cartItem.Products.FirstOrDefault(x => x.ID == id);

                if (productExists == null)
                {
                    _cartItem.Products.Add(new ProductModel { ID = productVm.ID, Name = productVm.Name, Quantity = productVm.Quantity, Price = productVm.Price });

                    refreshCache(_cartItem);

                    return Ok(new ServiceResponseVM() { Success = true });
                }
                else
                    return StatusCode(501, new ServiceResponseVM() { Success = false });
            }
            else
                return StatusCode(404, new ServiceResponseVM() { Success = false });
        }

        [HttpDelete]
        [Route("cart/item/{id:int}")]
        public IActionResult RemoveItem(int id, [FromBody] ProductModel productVm)
        {
            if (_cache.TryGetValue(cartItemCacheKey, out CartModel _cartItem) && _cartItem != null)
            {
                var productModel = _cartItem.Products.FirstOrDefault(x => x.ID == id);

                if (productModel != null)
                {
                    _cartItem.Products.Remove(productModel);

                    refreshCache(_cartItem);

                    return Ok(new ServiceResponseVM() { Success = true });
                }
                else
                    return StatusCode(404, new ServiceResponseVM() { Success = false });
            }

            return Ok(new ServiceResponseVM() { Success = false });
        }

        [HttpPost]
        [Route("cart/checkout")]
        public IActionResult CheckoutItem()
        {
            if (_cache.TryGetValue(cartItemCacheKey, out CartModel _cartItem) && _cartItem != null)
            {
                var products = _cartItem.Products.ToList();


                foreach (var product in products)
                {
                    _cartItem.Products.Remove(product);
                }

                refreshCache(null);

                return Ok(new ServiceResponseVM() { Success = true, Data = products });
            }
            else
                return StatusCode(501, new ServiceResponseVM() { Success = false });
        }

        public void refreshCache(CartModel _cartItem)
        {
            _cache.Remove(cartItemCacheKey);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024);

            _cache.Set(cartItemCacheKey, _cartItem, cacheEntryOptions);
        }
    }
}
