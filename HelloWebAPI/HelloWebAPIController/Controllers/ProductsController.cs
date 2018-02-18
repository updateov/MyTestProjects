using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HelloWebAPIController.Models;

namespace HelloWebAPIController.Controllers
{
    public class ProductsController : ApiController
    {
        Product[] products = new Product[] 
        { 
            new Product { Id = 1, Name = "Tomato Soup", Category = "Groceries", Price = 1, Element = new ProductElement() { ElementName = "Name1", Num = 12 }}, 
            new Product { Id = 4, Name = "Yo-yo", Category = "Toys", Price = 3.75M , Element = new ProductElement() { ElementName = "Name2", Num = 13 }}, 
            new Product { Id = 2, Name = "Yo-yo-yo", Category = "Toys", Price = 3.75M , Element = new ProductElement() { ElementName = "Name3", Num = 14 }}, 
            new Product { Id = 3, Name = "Hammer", Category = "Hardware", Price = 16.99M , Element = new ProductElement() { ElementName = "Name4", Num = 15 }} 
        };

        public IEnumerable<Product> GetAllProducts()
        {
            return products;
        }

        public Product GetProductById(int id)
        {
            var product = products.FirstOrDefault((p) => p.Id == id);
            if (product == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return product;
        }

        public IEnumerable<Product> GetProductsByCategory(string category)
        {
            var toRet = products.Where(
                (p) => string.Equals(p.Category, category,
                    StringComparison.OrdinalIgnoreCase));

            return toRet;
        }

        public Product GetProductBy2Params(int ID, String str)
        {
            var prod = new Product()
            {
                Id = ID,
                Category = str,
                Name = str,
                Price = 0,
                Element = new ProductElement()
                {
                    Num = 111,
                    ElementName = "bla"
                }
            };

            return prod;
        }

        public Product GetProductBy2IntParams(int ID, int id2)
        {
            var prod = new Product()
            {
                Id = ID,
                Category = id2.ToString(),
                Name = id2.ToString(),
                Price = 0,
                Element = new ProductElement()
                {
                    Num = 111,
                    ElementName = "bla"
                }
            };

            return prod;
        }
    }
}
