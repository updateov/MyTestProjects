using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebConsumer.ServiceReference1;
namespace WebConsumer
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Service1Client obj = new Service1Client();
            Response.Write(obj.GetData(12));
        }
    }
}
