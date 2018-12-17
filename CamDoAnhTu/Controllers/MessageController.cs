using CamDoAnhTu.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CamDoAnhTu.Controllers
{
    public class MessageController : Controller
    {
        // GET: Message
        public ActionResult Index(int type,int typemsg)
        {
            ViewBag.type = type;
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                var result = ctx.Messages.Where(p=> p.type == typemsg).ToList();
                return View(result);
            }

        }

        [HttpPost]
        public ActionResult DeleteMessage(int id)
        {

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                var message = ctx.Messages.Where(p => p.type == id).FirstOrDefault();

                if (message != null)
                {
                    ctx.Messages.Remove(message);
                    ctx.SaveChanges();
                }
                result["status"] = "success";
                return Json(result);
            }
        }
    }
}