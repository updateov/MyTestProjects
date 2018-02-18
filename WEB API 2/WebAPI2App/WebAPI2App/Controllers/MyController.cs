using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebAPI2App.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPI2App.Controllers
{
    [Route("api/[controller]")]
    public class MyController : Controller
    {
        private readonly MyContext m_context;

        public MyController(MyContext context)
        {
            m_context = context;

            if (m_context.MyItems.Count() == 0)
            {
                m_context.MyItems.Add(new MyItem { Name = "Item1" });
                m_context.SaveChanges();
            }
        }

        [HttpGet]
        public IEnumerable<MyItem> GetAll()
        {
            return m_context.MyItems.ToList();
        }

        [HttpGet("{id})", Name = "GetMy")]
        public IActionResult GetById(long id)
        {
            var item = m_context.MyItems.FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return new ObjectResult(item);
        }
    }
}
