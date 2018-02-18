using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HelloWebAPIController2.Models;

namespace HelloWebAPIController2.Controllers
{
    public class BlablablaController : ApiController
    {
        AnotherProduct[] anotherproducts = new AnotherProduct[] 
        { 
            new AnotherProduct { Id = 1, ProductName = "Tomato Soup" }, 
            new AnotherProduct { Id = 4, ProductName = "Yo-yo" }, 
            new AnotherProduct { Id = 2, ProductName = "Yo-yo-yo" },
            new AnotherProduct { Id = 3, ProductName = "Hammer" }
        };

        public IEnumerable<AnotherProduct> GetAllProducts()
        {
            return anotherproducts;
        }

        public AnotherProduct GetProductById(int id)
        {
            var product = anotherproducts.FirstOrDefault((p) => p.Id == id);
            if (product == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return product;
        }

        public AnotherProduct GetProductBy2Params(int ID, String str)
        {
            var prod = new AnotherProduct()
            {
                Id = ID,
                ProductName = str
            };

            return prod;
        }
    }
}
