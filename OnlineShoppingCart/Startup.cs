using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnlineShoppingCart.Models.Product;
using Microsoft.Extensions.Caching.Memory;
using OnlineShoppingCart.Helpers;
using OnlineShoppingCart.Models;

namespace OnlineShoppingCart
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddMvc();

            services.AddMemoryCache();

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMemoryCache cache)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Shopping Cart");
            });


            var cacheEntryOptions = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.Normal);

            var catalogProductItems = JsonFileReader.ReadAsync<ProductModel>(@".\Data\Products.json");

            var catalogModel = new CatalogModel { Products = catalogProductItems };

            cache.Set("catalogProductItemCache", catalogModel, cacheEntryOptions);

        }
    }
}
