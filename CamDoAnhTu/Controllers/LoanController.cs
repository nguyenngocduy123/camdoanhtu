using CamDoAnhTu.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CamDoAnhTu.Controllers
{
    public class LoanController : Controller
    {
        // GET: Loan
        public ActionResult Index(string idcus,int type)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ViewBag.type = type;
                var list = ctx.Loans.Where(p => p.IDCus == idcus && p.Type == true).ToList();

                return View(list);
            }                
        }
        [HttpPost]
        public JsonResult DeteleLoan(int id)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                var loan = ctx.Loans.Where(p => p.ID == id).FirstOrDefault();

                if (loan != null)
                {
                    ctx.Loans.Remove(loan);
                    ctx.SaveChanges();
                }
                result["status"] = "success";
                return Json(result);
            }
        }
    }
}