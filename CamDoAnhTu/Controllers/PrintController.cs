using CamDoAnhTu.Models;
using Rotativa.MVC;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Mvc;
using Rotativa.MVC;

namespace CamDoAnhTu.Controllers
{
    public class PrintController : Controller
    {
        // GET: Print
        public ActionResult Index(string code)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                var item = ctx.Customers.Where(p => p.Code == code).FirstOrDefault();
                return new ViewAsPdf(item);
                
            }
        }

        public ActionResult PrintPage(string code)
        {
            return new ActionAsPdf("Index", new { code = code });

        }
    }
}