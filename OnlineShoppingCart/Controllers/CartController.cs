using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OnlineShoppingCart.Helpers;
using OnlineShoppingCart.Models;
using OnlineShoppingCart.Models.Product;

namespace OnlineShoppingCart
{

    [Route("api")]
    public class CartController : Controller
    {
        private const string cartItemCacheKey = "cartItemCache";
        private const string catalogProductItemCacheKey = "catalogProductItemCache";
        private IMemoryCache _cache;

        public CartController(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Get All Cart Products
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("cart")]
        public IActionResult Get()
        {
            try
            {
                var data = JsonFileReader.ReadAsync<CartModel>(@".\Data\Products.json");

                if (_cache.TryGetValue(cartItemCacheKey, out CartModel _cartItem) && _cartItem?.Products?.Count > 0)
                {
                    _cartItem.TotalCost = _cartItem.Products.Sum(x => x.Price * x.Quantity);

                    return Ok(new { success = true, products = _cartItem.Products, totalCost = _cartItem.TotalCost });
                }

                return StatusCode(501, new { Success = false });
            }
            catch (Exception ex)
            {
                // we need to log the exception here
                return StatusCode(501, new { Success = false });
            }
        }

        /// <summary>
        /// Get Cart product by productID
        /// 
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("cart/item/{id:int}")]
        public IActionResult GetbyId(int id)
        {
            if (_cache.TryGetValue(cartItemCacheKey, out CartModel cartItem) && cartItem?.Products?.Count > 0)
            {
                var product = cartItem.Products.FirstOrDefault(x => x.ID == id);

                if (product != null)
                    return Ok(new { products = new List<ProductModel> { product }, Success = true });
                else
                    return StatusCode(404, new { Success = false });
            }

            return StatusCode(404, new { Success = false });
        }

        /// <summary>
        /// Add item to Cart
        /// 
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        [Route("cart/item/{id:int}")]
        public IActionResult AddItem(int id)
        {
            if (_cache.TryGetValue(cartItemCacheKey, out CartModel _cartItem) && _cartItem?.Products?.Count > 0)
            {
                if (_cartItem.Products.Any(x => x.ID == id))
                    return StatusCode(501, new { Success = false, Message = "product is already added to cart" });

                else
                {
                    if (_cache.TryGetValue(catalogProductItemCacheKey, out CatalogModel _catalogProducts) && _catalogProducts != null)
                    {
                        var product = _catalogProducts.Products.FirstOrDefault(x => x.ID == id);

                        if (product != null)
                        {
                            _cartItem.Products.Add(product);

                            RemoveItemFromCatalog(product);

                            UpdateCache(_cartItem);

                            return Ok(new { Success = true });

                        }
                        else
                            return StatusCode(404, new { Success = false });
                    }
                    return StatusCode(404, new { Success = false });
                }
            }
            else
            {
                if (_cache.TryGetValue(catalogProductItemCacheKey, out CatalogModel _catalogProducts) && _catalogProducts != null)
                {
                    var product = _catalogProducts.Products.FirstOrDefault(x => x.ID == id);

                    if (product != null)
                    {
                        _cartItem = new CartModel { Products = new List<ProductModel> { } };

                        _cartItem.Products.Add(product);

                        RemoveItemFromCatalog(product);

                        UpdateCache(_cartItem);

                        return Ok(new { Success = true });
                    }
                    else
                        return StatusCode(404, new { Success = false });
                }

                return StatusCode(404, new { Success = false });
            }
        }

        /// <summary>
        /// Remove item from Cart
        /// 
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [Route("cart/item/{id:int}")]
        public IActionResult RemoveItem(int id)
        {
            if (_cache.TryGetValue(cartItemCacheKey, out CartModel _cartItem) && _cartItem?.Products?.Count > 0)
            {
                var productModel = _cartItem.Products.FirstOrDefault(x => x.ID == id);

                if (productModel != null)
                {
                    _cartItem.Products.Remove(productModel);

                    UpdateCache(_cartItem);

                    AddItemToCatalog(productModel);

                    return Ok(new { Success = true });
                }
                else
                    return StatusCode(404, new { Success = false });
            }

            return StatusCode(404, new { Success = false });
        }

        /// <summary>
        /// Checkout from Cart
        /// 
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        [Route("cart/checkout")]
        public IActionResult CheckoutItem()
        {
            if (_cache.TryGetValue(cartItemCacheKey, out CartModel _cartItem) && _cartItem?.Products?.Count > 0)
            {
                var products = _cartItem.Products.ToList();

                foreach (var product in products)
                {
                    _cartItem.Products.Remove(product);

                    //AddItemToCatalog(product);
                }

                _cartItem.TotalCost = products.Sum(x => x.Price * x.Quantity);

                 UpdateCache(null);

                return Ok(new { success = true, products = products, totalCost = _cartItem.TotalCost });
            }
            else
                return StatusCode(501, new { Success = false });
        }

        
        private void UpdateCache(CartModel _cartItem)
        {
            _cache.Remove(cartItemCacheKey);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024);

            _cache.Set(cartItemCacheKey, _cartItem, cacheEntryOptions);
        }

        
        private void RemoveItemFromCatalog(ProductModel product)
        {
            if (_cache.TryGetValue(catalogProductItemCacheKey, out CatalogModel _catalogProducts) && _catalogProducts != null)
            {
                _catalogProducts.Products.Remove(product);

                _cache.Remove(catalogProductItemCacheKey);

                _cache.Set(catalogProductItemCacheKey, _catalogProducts);
            }
        }

        
        private void AddItemToCatalog(ProductModel product)
        {
            if (_cache.TryGetValue(catalogProductItemCacheKey, out CatalogModel _catalogProducts) && _catalogProducts != null)
            {
                if (!_catalogProducts.Products.Any(x => x.ID == product.ID))
                {
                    _catalogProducts.Products.Add(product);

                    _cache.Remove(catalogProductItemCacheKey);

                    _cache.Set(catalogProductItemCacheKey, _catalogProducts);
                }
            }
        }
    }
}
