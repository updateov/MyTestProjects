using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAPIApp.Models;
using WebAPIApp.Services;
using System.Web.Http;

namespace WebAPIApp.Controllers
{
    public class ContactController : ApiController
    {
        private ContactRepository contactRepository;

        public ContactController()
        {
            this.contactRepository = new ContactRepository();
        }
        
        //
        // GET: /Contact/
        public Contact[] Get()
        {
            return contactRepository.GetAllContacts();
        }
    }
}
