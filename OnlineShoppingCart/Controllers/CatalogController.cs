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
        private readonly IProductListHandler _handler;
        private readonly IMapper _mapper;

        public CatalogController(IProductListHandler handler, IMapper mapper)
        {
            _handler = handler;
            _mapper = mapper;
        }

        // GET: api/Product
        [HttpGet]
        [Route("api/catalog/size")]
        public IActionResult Get()
        {
            try
            {
                var query = new ProductListQuery();

                var catalogItems = JsonFileReader.ReadAsync<CatalogModel>(@".\Data\Catalog.json");

                var results = catalogItems.FirstOrDefault().Products;

                var response = new ServiceResponseVM()
                {
                    Success = true,
                };

                if (results.Count > 0)
                    response.count = results.Count;
                else
                    response.Data = "No items in catalog";

                return Ok(response);
            }
            catch (FileNotFoundException ex)
            {
                var response = new ServiceResponseVM()
                {
                    Success = false,
                    Data = ex.Message
                };

                return StatusCode(501, response);
            }
        }

        [HttpGet]
        [Route("api/catalog/{catalogId:int}")]
        public IActionResult GetbyId(int catalogId)
        {
            CatalogModel catalogItem = null;

            List<ProductModel> result = null;

            if (catalogItem.CatalogId == catalogId)
            {
                result = catalogItem.Products;
            }

            if (result != null)
                return Ok(new ServiceResponseVM() { Success = true, Data = result, count = result.Count });
            else
                return StatusCode(404, new ServiceResponseVM() { Success = false });
        }
    }
}
