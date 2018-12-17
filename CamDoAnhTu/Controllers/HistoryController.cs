using CamDoAnhTu.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamDoAnhTu.Controllers
{
    public class HistoryController : Controller
    {
        // GET: History
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult History(string id, int? type)
        {
            if (type.HasValue)
            {
                ViewBag.type = type.Value;
                using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
                {
                    List<history> lstHistory = ctx.histories.Where(p => p.CustomerId == id).ToList();

                    return View(lstHistory);
                }
            }
            else
            {
                ViewBag.type = -2;
                using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
                {
                    List<history> lstHistory = ctx.histories.Where(p => p.CustomerId == id).ToList();

                    return View(lstHistory);
                }
            }


        }
    }
}