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


    public class CatalogController : Controller
    {
        private const string catalogProductItemCacheKey = "catalogProductItemCache";
        private IMemoryCache _cache;

        public CatalogController(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Get Catalog by Size, it returns the catalog products count
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/catalog/size")]
        public IActionResult Get()
        {
            try
            {
                var catalogProductItems = JsonFileReader.ReadAsync<ProductModel>(@".\Data\Products.json");

                if (_cache.TryGetValue(catalogProductItemCacheKey, out CatalogModel _catalogProducts) && _catalogProducts != null)
                {
                    if (_catalogProducts.Products.Count > 0)
                        return Ok(new { Success = true, count = _catalogProducts.Products.Count });
                }

                return Ok(new { Success = true, count = 0 });
            }
            catch (FileNotFoundException ex)
            {
                //log the error with ex.stacktrace
                return StatusCode(501, new { Success = false });
            }
        }

        /// <summary>
        /// Get Catalog Product by Id, it returns the catalog product
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/catalog/{productId:int}")]
        public IActionResult GetbyId(int productId)
        {
            if (_cache.TryGetValue(catalogProductItemCacheKey, out CatalogModel _catalogProducts) && _catalogProducts != null)
            {
                var product = _catalogProducts.Products.FirstOrDefault(x => x.ID == productId);

                if (product != null)
                    return Ok(new { products = new List<ProductModel> { product } ,Success = true });
                else
                    return StatusCode(404, new { Success = false });
            }
            else
                return StatusCode(404, new { Success = false });
        }
    }
}
