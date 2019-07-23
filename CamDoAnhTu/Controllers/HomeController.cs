using CamDoAnhTu.Helper;
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
    [SessionTimeout]
    public class HomeController : Controller
    {
        private static int update = 0;

        // GET: Home
        public ActionResult Index()
        {           

            return View();
        }

        public ActionResult XE1()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                List<Customer> list = ctx.Customers.Where(p => p.type == 12).ToList();

                return PartialView(list);
            }
        }

        public DateTime dtcompareNewCs = DateTime.ParseExact("28/06/2018", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        public DateTime dtcompareOldCs = DateTime.ParseExact("30/06/1990", "dd/MM/yyyy", CultureInfo.InvariantCulture);

        public ActionResult Management(int type)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ViewBag.type = type;
                List<Customer> list = new List<Customer>();

                if (type == -1)
                {
                    list = ctx.Customers.Where(p => p.type != 12
                    && p.StartDate >= dtcompareOldCs && p.IsDeleted == false).ToList();
                    return PartialView(list);
                }
                else if (type == -2) // nhung thang khach hang moi
                {
                    list = ctx.Customers.Where(p => p.type != 12
                    && p.StartDate >= dtcompareNewCs && p.IsDeleted == false).ToList();
                    return PartialView(list);

                }
                else if (type == 1 || type == 2 || type == 3 || type == 4 || type == 5 || type == 6)
                {
                    list = ctx.Customers.Where(p => p.type == type
                    && p.StartDate >= dtcompareNewCs && p.IsDeleted == false).ToList();

                    switch (type)
                    {
                        case 1:
                            var list1 = ctx.Customers.Where(p => p.type == type
                    && p.StartDate >= dtcompareNewCs).ToList();
                            break;
                        default:
                            break;
                    }
                }

                return PartialView(list);
            }
        }

        public ActionResult LoadCustomer(int? pageSize, int? type, int page = 1)
        {

            if (Session["User"] == null)
                return RedirectToAction("Login", "Account");

            int pageSz = pageSize ?? 10;
            StringBuilder str = new StringBuilder();
            decimal k = 0;

            int todayYear = DateTime.Now.Year;
            int todayMonth = DateTime.Now.Month;
            int todayDay = DateTime.Now.Day;

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {

                ctx.Configuration.ValidateOnSaveEnabled = false;
                var tiengoc = ctx.GetTienGoc(type).SingleOrDefault();

                var lishExample = ctx.Customers.Where(p => p.type == 4).ToList();

                if (tiengoc != null)
                {
                    ViewBag.tiengoc = $"{tiengoc.Value:N0}";
                }

                var tienlai = ctx.GetTienLai(type).SingleOrDefault();
                var tienlaithucte = ctx.GetTienLaiThatTe(type).SingleOrDefault();

                if (tienlai != null)
                {
                    ViewBag.tienlai = $"{tienlai.Value:N0}";
                }
                if (tienlaithucte != null)
                {
                    //save tien lai

                    var message = ctx.Messages.Where(p => p.Date == DateTime.Now && DateTime.Now.Day == 1).FirstOrDefault();
                    if (message == null)
                    {
                        Message newMsg = new Message();
                        newMsg.Message1 = $"Tiền lãi thực tế : {tienlaithucte} ";
                        newMsg.type = 2;
                        newMsg.Date = DateTime.Now;
                        ctx.Messages.Add(newMsg);
                        ctx.SaveChanges();
                    }

                    ViewBag.tienlaithucte = string.Format("{0:N0}", tienlaithucte.Value);
                }
                List<Customer> list1 = new List<Customer>();

                if (type == -1)
                {
                    list1 = ctx.Customers.Where(p => p.IsDeleted == false
                    && p.type != 12
                    && (p.Description == "End" || p.NgayNo < 59)
                    && p.StartDate >= dtcompareOldCs).ToList();

                }
                else if (type == -2) // nhung thang khach hang moi
                {
                    list1 = ctx.Customers.Where(p => p.IsDeleted == false
                                                     && p.type != 12
                                                     && (p.Description == "End" || p.DayPaids == (p.Loan / p.Price) || p.NgayNo < 59)
                                                     && p.StartDate >= dtcompareNewCs).ToList();


                }
                else if (type == 1 || type == 2 || type == 3 || type == 4 || type == 5 || type == 6)
                {
                    list1 = ctx.Customers.Where(p => p.IsDeleted == false
                                && p.type == type
                                && (p.Description == "End" || p.DayPaids == (p.Loan / p.Price) || p.NgayNo < 59)
                                && p.StartDate >= dtcompareNewCs).ToList();
                }

                int count1 = list1.Count();
                int nPages = count1 / pageSz + (count1 % pageSz > 0 ? 1 : 0);

                List<Customer> list = list1.OrderBy(p => p.CodeSort)
                    .Skip((page - 1) * pageSz)
                     .Take(pageSz).ToList();

                ViewBag.PageCount = nPages;
                ViewBag.type = type.Value;
                ViewBag.CurPage = page;

                var summoney = (from l in ctx.Loans
                                join cs in ctx.Customers on l.IDCus equals cs.Code
                                where (cs.type == type || type == -1 || type == -2)
                                && cs.StartDate >= dtcompareNewCs && l.Date.Year == todayYear
                                && l.Date.Month == todayMonth
                                && l.Date.Day == todayDay
                                && cs.Description != "End"
                                select new
                                {
                                    cs.Price,
                                    cs.Code
                                }).ToList();

                foreach (var x in summoney)
                {
                    k += x.Price ?? 0;
                }

                foreach (Customer cs in list)
                {
                    cs.NgayNo = 0;
                    int count = 0;
                    int countMax = 0;

                    DateTime EndDate = DateTime.Now;

                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    Loan t1 = new Loan();

                    if (t.Count != 0)
                    {
                        t1 = t.First();
                    }

                    DateTime StartDate = t1.Date;

                    List<Loan> query = t.Where(p => p.Date >= StartDate && p.Date <= EndDate).ToList();

                    foreach (Loan temp in query)
                    {
                        if (temp.Status == 0)
                        {
                            count++;
                            countMax = count;
                        }
                        else
                        {
                            count = 0;
                        }
                    }

                    cs.NgayNo = countMax;
                    ctx.SaveChanges();
                }

                str.Append("Số tiền phải thu trong ngày " + DateTime.Now.Date.ToShortDateString() + " : " + k.ToString());
                ViewBag.Message1 = str.ToString();
                if (TempData["message"] != null)
                {
                    ViewBag.Message = TempData["message"].ToString();
                }

                return View(list);
            }
        }

        public ActionResult BadCustomer(int? pageSize, int type, int page = 1)
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "Account");

            int pageSz = pageSize ?? 10;
            StringBuilder str = new StringBuilder();
            decimal? k = 0;

            int todayYear = DateTime.Now.Year;
            int todayMonth = DateTime.Now.Month;
            int todayDay = DateTime.Now.Day;

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                List<Customer> list1 = new List<Customer>();

                if (type == -1)
                {
                    list1 = ctx.Customers.Where(p => p.type != 12 && p.Loans.OrderByDescending(x => x.ID)
                    .FirstOrDefault().Date > DateTime.Now).ToList();
                }
                else
                {
                    list1 = ctx.Customers.Where(p => p.type == type && p.Loans.OrderByDescending(x => x.ID)
                    .FirstOrDefault().Date > DateTime.Now).ToList();
                }

                int count1 = list1.Count();
                int nPages = count1 / pageSz + (count1 % pageSz > 0 ? 1 : 0);

                List<Customer> list = list1.OrderBy(p => p.CodeSort)
                    .Skip((page - 1) * pageSz)
                     .Take(pageSz).ToList();

                ViewBag.type = type;
                ViewBag.PageCount = nPages;
                ViewBag.CurPage = page;

                var summoney = (from l in ctx.Loans
                                join cs in ctx.Customers on l.IDCus equals cs.Code
                                where cs.type == type && l.Date.Year == todayYear && l.Date.Month == todayMonth && l.Date.Day == todayDay
                                select new
                                {
                                    cs.Price
                                }).ToList();

                foreach (var x in summoney)
                {
                    k += x.Price;
                }

                foreach (Customer cs in list)
                {
                    cs.NgayNo = 0;
                    int count = 0;
                    int countMax = 0;

                    DateTime EndDate = DateTime.Now;

                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    Loan t1 = new Loan();

                    if (t.Count != 0)
                    {
                        t1 = t.First();
                    }

                    DateTime StartDate = t1.Date;

                    List<Loan> query = t.Where(p => p.Date >= StartDate && p.Date <= EndDate).ToList();

                    foreach (Loan temp in query)
                    {
                        if (temp.Status == 0)
                        {
                            count++;
                            countMax = count;
                        }
                        else
                        {
                            count = 0;
                        }
                    }

                    cs.NgayNo = countMax;
                    ctx.SaveChanges();
                }

                str.Append("Số tiền phải thu trong ngày " + DateTime.Now.Date.ToShortDateString() + " : " + k.ToString());

                if (TempData["message"] != null)
                {
                    ViewBag.Message = TempData["message"].ToString();
                }

                return View(list);
            }
        }

        public ActionResult LoadCustomerXE1(int? pageSize, int page = 1)
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "Account");

            int pageSz = pageSize ?? 10;

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                List<Customer> list1 = ctx.Customers.Where(p => p.type == 12 && p.IsDeleted == false).ToList();
                int count1 = list1.Count();
                int nPages = count1 / pageSz + (count1 % pageSz > 0 ? 1 : 0);

                List<Customer> list = list1.OrderBy(p => p.CodeSort)
                    .Skip((page - 1) * pageSz)
                     .Take(pageSz).ToList();

                ViewBag.PageCount = nPages;
                ViewBag.CurPage = page;

                foreach (Customer cs in list1)
                {
                    cs.nodung = false;
                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    foreach (var item in t)
                    {
                        DateTime tempDT = item.Date;

                        if (tempDT.Date <= DateTime.Now.Date && item.Status == 0)
                        {
                            cs.nodung = true;
                        }
                    }

                    ctx.SaveChanges();
                }

                return View(list);
            }
        }

        public ActionResult Search(string Code, string Name, string Phone,
            string Address, int? Noxau, int? hetno, int page = 1, int type = -1)
        {
            int pageSz = Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]);
            List<Loan> lstLoan = new List<Loan>();

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                var list = ctx.Customers.ToList();
                List<Customer> lsttrave = new List<Customer>();

                if (type != -1)
                {
                    lsttrave = list.Where(p => p.type == type).ToList();
                }

                if (!string.IsNullOrEmpty(Code))
                {
                    lsttrave = list.Where(p => p.Code == Code || p.Code.StartsWith(Code.ToUpper())).ToList();
                }
                if (!string.IsNullOrEmpty(Name))
                {
                    lsttrave = list.Where(p => p.Name.Contains(Name)).ToList();
                }
                if (!string.IsNullOrEmpty(Phone))
                {
                    lsttrave = list.Where(p => p.Phone.Contains(Phone)).ToList();
                }
                if (!string.IsNullOrEmpty(Address))
                {
                    lsttrave = list.Where(p => p.Address.Contains(Address)).ToList();
                }

                lsttrave = lsttrave.Where(p => p.IsDeleted == false).ToList();

                if (Noxau == 1)
                {
                    List<Customer> lstCus = ctx.Customers.Where(p => p.Price != null && p.Loan != null).ToList();
                    foreach (Customer p in lstCus)
                    {
                        if (p.NgayNo >= 3 + Int32.Parse(p.DayBonus.ToString()))
                        {
                            lsttrave.Add(p);
                        }
                    }
                }

                if (hetno == 1)
                {
                    List<Customer> lstCus = ctx.Customers.Where(p => p.Price != null && p.Loan != null).ToList();
                    foreach (Customer p in lstCus)
                    {
                        int day = 0;
                        if (Int32.Parse(p.Price.ToString()) == 0)
                        {
                            day = 0;
                        }
                        else
                            day = Int32.Parse(p.Loan.ToString()) / Int32.Parse(p.Price.ToString());

                        if (p.DayPaids == day || p.Description == "End")
                        {
                            lsttrave.Add(p);
                        }
                    }
                }

                int count = lsttrave.Count();
                int nPages = count / pageSz + (count % pageSz > 0 ? 1 : 0);
                List<Customer> lsttrave1 = lsttrave.OrderBy(p => p.CodeSort)
                    .Skip((page - 1) * pageSz)
                     .Take(pageSz).ToList();

                ViewBag.PageCount = nPages;
                ViewBag.CurPage = page;
                ViewBag.Code = Code;
                ViewBag.Name = Name;
                ViewBag.Phone = Phone;
                ViewBag.Noxau = Noxau;
                ViewBag.hetno = hetno;
                ViewBag.Address = Address;
                ViewBag.type = type;

                foreach (Customer cs in lsttrave1)
                {
                    cs.NgayNo = 0;
                    int count1 = 0;
                    int countMax = 0;

                    DateTime EndDate = DateTime.Now;

                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    Loan t1 = new Loan();

                    if (t.Count != 0)
                    {
                        t1 = t.First();
                    }

                    DateTime StartDate = t1.Date;

                    List<Loan> query = t.Where(p => p.Date >= StartDate && p.Date <= EndDate).ToList();

                    foreach (Loan temp in query)
                    {
                        if (temp.Status == 0)
                        {
                            count1++;
                            countMax = count1;
                        }
                        else
                        {
                            count1 = 0;
                        }
                    }

                    cs.NgayNo = countMax;
                    ctx.SaveChanges();
                }

                return View(lsttrave1);
            }
        }

        public ActionResult SearchXE1(string Code, string Name, string Phone, string Address, string tentaisan, int? Noxau, int? hetno, int page = 1)
        {
            int pageSz = Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]);
            List<Loan> lstLoan = new List<Loan>();

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                var list = ctx.Customers.Where(p => p.type == 12).ToList();
                List<Customer> lsttrave = new List<Customer>();

                if (!string.IsNullOrEmpty(Code))
                {
                    lsttrave = list.Where(p => p.Code == Code).ToList();
                }
                if (!string.IsNullOrEmpty(Name))
                {
                    lsttrave = list.Where(p => p.Name.Contains(Name)).ToList();
                }
                if (!string.IsNullOrEmpty(Phone))
                {
                    lsttrave = list.Where(p => p.Phone.Contains(Phone)).ToList();
                }

                if (!string.IsNullOrEmpty(tentaisan))
                {
                    lsttrave = list.Where(p => p.tentaisan.Contains(tentaisan)).ToList();
                }
                if (Noxau == 1)
                {
                    List<Customer> lstCus = ctx.Customers.Where(p => p.type == 12).ToList();
                    foreach (Customer p in lstCus)
                    {
                        if (p.nodung == true)
                        {
                            lsttrave.Add(p);
                        }
                    }
                }

                if (hetno == 1)
                {
                    List<Customer> lstCus = ctx.Customers.Where(p => p.type == 12).ToList();
                    foreach (Customer p in lstCus)
                    {
                        if (p.nodung == false)
                        {
                            lsttrave.Add(p);
                        }
                    }
                }

                int count = lsttrave.Count();
                int nPages = count / pageSz + (count % pageSz > 0 ? 1 : 0);
                List<Customer> lsttrave1 = lsttrave.OrderBy(p => p.CodeSort)
                    .Skip((page - 1) * pageSz)
                     .Take(pageSz).ToList();

                ViewBag.PageCount = nPages;
                ViewBag.CurPage = page;
                ViewBag.Code = Code;
                ViewBag.Name = Name;
                ViewBag.Phone = Phone;
                ViewBag.Noxau = Noxau;
                ViewBag.hetno = hetno;
                ViewBag.Address = Address;
                ViewBag.tentaisan = tentaisan;

                foreach (Customer cs in lsttrave1)
                {
                    cs.NgayNo = 0;
                    int count1 = 0;
                    int countMax = 0;

                    DateTime EndDate = DateTime.Now;

                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    Loan t1 = new Loan();

                    if (t.Count != 0)
                    {
                        t1 = t.First();
                    }

                    DateTime StartDate = t1.Date;

                    List<Loan> query = t.Where(p => p.Date >= StartDate && p.Date <= EndDate).ToList();

                    foreach (Loan temp in query)
                    {
                        if (temp.Status == 0)
                        {
                            count1++;
                            countMax = count1;
                        }
                        else
                        {
                            count1 = 0;
                        }
                    }

                    cs.NgayNo = countMax;
                    ctx.SaveChanges();
                }

                return View(lsttrave1);
            }
        }

        public ActionResult Refresh(int type)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                var query1 = ctx.Customers.ToList();

                foreach (Customer cs in query1)
                {
                    cs.NgayNo = 0;
                    int count = 0;
                    int countMax = 0;

                    DateTime EndDate = DateTime.Now;

                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    Loan t1 = new Loan();

                    if (t.Count != 0)
                    {
                        t1 = t.First();
                    }

                    DateTime StartDate = t1.Date;

                    List<Loan> query = t.Where(p => p.Date >= StartDate && p.Date <= EndDate).ToList();

                    foreach (Loan temp in query)
                    {
                        if (temp.Status == 0)
                        {
                            count++;
                            countMax = count;
                        }
                        else
                        {
                            count = 0;
                        }
                    }

                    cs.NgayNo = countMax;
                    ctx.SaveChanges();
                }
            }
            return RedirectToAction("LoadCustomer", "Home", new { type = type });
        }
        public ActionResult AddCustomer(int type)
        {

            ViewBag.ListLoaiGiayTo = new SelectList(
                new List<SelectListItem>
                {
                        new SelectListItem { Text = "Giấy tờ chính chủ", Value = "1"},
                        new SelectListItem { Text = "Giấy tờ photo", Value = "2"},
                        new SelectListItem { Text = "Không có giấy tờ", Value = "3"}
                }, "Value", "Text");

            MyViewModel mvViewModel = new MyViewModel();
            mvViewModel.model.type = type;

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {

            }
            return View(mvViewModel);
        }

        [HttpPost]
        public ActionResult AddCustomer(MyViewModel myViewModel)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ViewBag.ListLoaiGiayTo = new SelectList(
                new List<SelectListItem>
                {
                        new SelectListItem { Text = "Giấy tờ chính chủ", Value = "1"},
                        new SelectListItem { Text = "Giấy tờ photo", Value = "2"},
                        new SelectListItem { Text = "Không có giấy tờ", Value = "3"}
                }, "Value", "Text");
                ctx.Configuration.ValidateOnSaveEnabled = false;

                float day = float.Parse(myViewModel.model.Loan.ToString()) / float.Parse(myViewModel.model.Price.ToString());

                if (day != (int)day)
                {
                    ViewBag.Message = "Số ngày không hợp lệ";
                    return View(myViewModel);
                }

                if (day > 60)
                {
                    ViewBag.Message = "Số ngày nợ lớn hơn 60";
                    return View(myViewModel);
                }

                myViewModel.model.DayPaids = 0;
                myViewModel.model.AmountPaid = 0;
                myViewModel.model.RemainingAmount = myViewModel.model.Loan;
                //myViewModel.model.type = 0;
                myViewModel.model.loaigiayto = myViewModel.SelectedLoaiGiayTo;
                myViewModel.model.NgayNo = 0;
                myViewModel.model.DayBonus = myViewModel.model.DayBonus ?? 0;

                if (myViewModel.model.Code.Contains("ZA") || myViewModel.model.Code.Contains("ZB") || myViewModel.model.Code.Contains("ZC"))
                {
                    int code = Int32.Parse(myViewModel.model.Code.Substring(2, myViewModel.model.Code.Length - 2));

                    if (myViewModel.model.Code[1] == 'A')
                    {
                        myViewModel.model.CodeSort = code + 1000;
                    }
                    else
                    {
                        myViewModel.model.CodeSort = (((myViewModel.model.Code[1] - 'A') + 1) * 1000) + code;
                    }
                }
                else if ((myViewModel.model.Code[0] >= 'A' && myViewModel.model.Code[0] <= 'Z') ||
                    (myViewModel.model.Code[0] >= 'a' && myViewModel.model.Code[0] <= 'z') && myViewModel.model.CodeSort == null)
                {
                    int code = Int32.Parse(myViewModel.model.Code.Substring(1, myViewModel.model.Code.Length - 1));

                    if (myViewModel.model.Code[0] == 'A')
                    {
                        myViewModel.model.CodeSort = code + 1000;
                    }
                    else
                    {
                        myViewModel.model.CodeSort = (((myViewModel.model.Code[0] - 'A') + 1) * 1000) + code;
                    }
                }
                else
                {
                    myViewModel.model.CodeSort = Int32.Parse(myViewModel.model.Code);
                }

                ctx.Customers.Add(myViewModel.model);
                ctx.SaveChanges();

                for (int i = 1; i <= day; i++)
                {
                    Loan temp = new Loan();
                    temp.Date = myViewModel.model.StartDate.AddDays(i);
                    temp.IDCus = myViewModel.model.Code;
                    temp.Status = 0;
                    ctx.Loans.Add(temp);
                    ctx.SaveChanges();
                }
            }

            if (myViewModel.fuMain != null && myViewModel.fuMain.ContentLength > 0)
            {
                string spDirPath = Server.MapPath("~/image");
                string targetDirPath = Path.Combine(spDirPath, myViewModel.model.Code.ToString());
                Directory.CreateDirectory(targetDirPath);

                string mainFileName = Path.Combine(targetDirPath, "main.jpg");
                //myViewModel.fuMain.SaveAs(mainFileName);

                Image bm = Image.FromStream(myViewModel.fuMain.InputStream);
                bm = Helper.Helper.ResizeBitmap((Bitmap)bm, 160, 160); /// new width, height
                bm.Save(Path.Combine(targetDirPath, "main.jpg"));
            }
            return RedirectToAction("LoadCustomer", "Home", new { type = myViewModel.model.type });
        }

        public ActionResult Update(string id)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                MyViewModel viewModel = new MyViewModel();
                ViewBag.ListLoaiGiayTo = new SelectList(
                    new List<SelectListItem>
                    {
                        new SelectListItem { Text = "Giấy tờ chính chủ", Value = "1"},
                        new SelectListItem { Text = "Giấy tờ photo", Value = "2"},
                        new SelectListItem { Text = "Không có giấy tờ", Value = "3"}
                    }, "Value", "Text");

                update = 0;
                Customer pro = ctx.Customers.Where(p => p.Code == id).FirstOrDefault();
                if (pro.Loan == null || pro.Price == null)
                {
                    update = 1;
                }

                if (pro == null)
                {
                    return RedirectToAction("LoadCustomer", "Home");
                }
                viewModel.model = pro;
                viewModel.SelectedLoaiGiayTo = pro.loaigiayto;
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Update(MyViewModel myViewModel)
        {
            try
            {

                int i = 1;

                if (myViewModel.model.DayBonus == null)
                {
                    myViewModel.model.DayBonus = 0;
                }

                using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
                {
                    ctx.Configuration.ValidateOnSaveEnabled = false;
                    Customer pro = ctx.Customers.Where(p => p.Code == myViewModel.model.Code).FirstOrDefault();
                    pro.Name = myViewModel.model.Name;
                    pro.Phone = myViewModel.model.Phone;
                    pro.Address = myViewModel.model.Address;
                    pro.Loan = myViewModel.model.Loan;
                    pro.Price = myViewModel.model.Price;
                    pro.DayPaids = myViewModel.model.DayPaids;
                    pro.AmountPaid = myViewModel.model.AmountPaid;
                    pro.RemainingAmount = myViewModel.model.RemainingAmount;
                    pro.DayBonus = myViewModel.model.DayBonus;
                    pro.OldCode = myViewModel.model.OldCode;
                    pro.Note = myViewModel.model.Note;
                    pro.loaigiayto = myViewModel.SelectedLoaiGiayTo;
                    pro.tiengoc = myViewModel.model.tiengoc;

                    if (pro.StartDate != myViewModel.model.StartDate)
                    {
                        pro.StartDate = myViewModel.model.StartDate;
                        int t = Int32.Parse(myViewModel.model.Loan.ToString()) / Int32.Parse(myViewModel.model.Price.ToString());
                        List<Loan> l = ctx.Loans.Where(p => p.IDCus == myViewModel.model.Code).ToList();

                        foreach (Loan temp in l)
                        {
                            temp.Date = myViewModel.model.StartDate.AddDays(i);
                            temp.IDCus = myViewModel.model.Code;
                            temp.Status = 0;
                            i++;
                            ctx.SaveChanges();
                        }
                    }

                    ctx.SaveChanges();

                    if (update == 1)
                    {
                        pro = ctx.Customers.Where(p => p.Code == pro.Code).FirstOrDefault();

                        int day = Int32.Parse(pro.Loan.ToString()) / Int32.Parse(pro.Price.ToString());

                        for (int s = 1; s <= day; s++)
                        {
                            Loan temp = new Loan();
                            temp.Date = pro.StartDate.AddDays(s);
                            temp.IDCus = pro.Code;
                            temp.Status = 0;
                            ctx.Loans.Add(temp);
                            ctx.SaveChanges();
                        }
                        ctx.SaveChanges();
                    }
                }

                if (myViewModel.fuMain != null && myViewModel.fuMain.ContentLength > 0)
                {

                    string spDirPath = Server.MapPath("~/image");
                    string targetDirPath = Path.Combine(spDirPath, myViewModel.model.Code.ToString());
                    //Directory.CreateDirectory(targetDirPath);

                    //string mainFileName = Path.Combine(targetDirPath, "main.jpg");
                    //myViewModel.fuMain.SaveAs(mainFileName);

                    Image bm = Image.FromStream(myViewModel.fuMain.InputStream);
                    bm = Helper.Helper.ResizeBitmap((Bitmap)bm, 160, 160); /// new width, height
                    bm.Save(Helper.Helper.GetAbsoluteFilePath("main.jpg", myViewModel));
                    //bm.Save(Path.Combine(targetDirPath, "main.jpg"));

                }

                return RedirectToAction("LoadCustomer", "Home", new { type = myViewModel.model.type });
            }
            catch (Exception)
            {
                return View();
            }
        }

        public ActionResult AddCustomerXE1()
        {
            ViewBag.ListLoaiGiayTo = new SelectList(
                new List<SelectListItem>
                {
                        new SelectListItem { Text = "Giấy tờ chính chủ", Value = "1"},
                        new SelectListItem { Text = "Giấy tờ photo", Value = "2"},
                        new SelectListItem { Text = "Không có giấy tờ", Value = "3"}
                }, "Value", "Text");

            return View();
        }

        [HttpPost]
        public ActionResult AddCustomerXE1(MyViewModel myViewModel)
        {
            var model = myViewModel.model;
            model.loaigiayto = myViewModel.SelectedLoaiGiayTo;
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                List<Loan> lstLoan = new List<Loan>();

                model.type = 12;
                if (model.Code.Contains("ZA") || model.Code.Contains("ZB") || model.Code.Contains("ZC"))
                {
                    int code;

                    if (model.Code.Length - 1 == 2)
                        code = Int32.Parse(model.Code[2].ToString());
                    else
                        code = Int32.Parse(model.Code.Substring(2, model.Code.Length - 2));
                    model.CodeSort = 50000 + code;
                }
                else if ((model.Code[0] >= 'A' && model.Code[0] <= 'Z') || (model.Code[0] >= 'a' && model.Code[0] <= 'z') && model.CodeSort == null)
                {
                    int code = Int32.Parse(model.Code.Substring(1, model.Code.Length - 1));

                    if (model.Code[0] == 'A')
                    {
                        model.CodeSort = code + 1000;
                    }
                    else
                    {
                        model.CodeSort = (((model.Code[0] - 'A') + 1) * 1000) + code;
                    }
                }
                else
                {
                    model.CodeSort = Int32.Parse(model.Code);
                }

                ctx.Customers.Add(model);
                ctx.SaveChanges();

                int day = model.songayno.HasValue ? (int)model.songayno : 1;
                DateTime k = model.StartDate;

                for (int i = 1; i <= 60; i++)
                {
                    Loan temp = new Loan();
                    temp.Date = k.AddDays(day);
                    temp.IDCus = model.Code;
                    temp.Status = 0;
                    ctx.Loans.Add(temp);

                    k = temp.Date;
                    lstLoan.Add(temp);

                    ctx.SaveChanges();
                }
                ViewData["Loans"] = lstLoan;
            }

            if (myViewModel.fuMain != null && myViewModel.fuMain.ContentLength > 0)
            {
                string spDirPath = Server.MapPath("~/image");
                string targetDirPath = Path.Combine(spDirPath, myViewModel.model.Code.ToString());
                Directory.CreateDirectory(targetDirPath);

                string mainFileName = Path.Combine(targetDirPath, "main.jpg");
                //myViewModel.fuMain.SaveAs(mainFileName);

                Image bm = Image.FromStream(myViewModel.fuMain.InputStream);
                bm = Helper.Helper.ResizeBitmap((Bitmap)bm, 160, 160); /// new width, height
                bm.Save(Path.Combine(targetDirPath, "main.jpg"));
            }
            return RedirectToAction("LoadCustomerXE1", "Home");
        }

        public ActionResult UpdateXE1(string id)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                MyViewModel viewModel = new MyViewModel();
                ViewBag.ListLoaiGiayTo = new SelectList(
                    new List<SelectListItem>
                    {
                        new SelectListItem { Text = "Giấy tờ chính chủ", Value = "1"},
                        new SelectListItem { Text = "Giấy tờ photo", Value = "2"},
                        new SelectListItem { Text = "Không có giấy tờ", Value = "3"}
                    }, "Value", "Text");

                update = 0;
                Customer pro = ctx.Customers.Where(p => p.Code == id).FirstOrDefault();
                if (pro.Loan == null || pro.Price == null)
                {
                    update = 1;
                }

                if (pro == null)
                {
                    return RedirectToAction("LoadCustomer", "Home");
                }
                viewModel.model = pro;
                viewModel.SelectedLoaiGiayTo = pro.loaigiayto;
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult UpdateXE1(MyViewModel myViewModel)
        {
            var model = myViewModel.model;
            if (model.DayBonus == null)
            {
                model.DayBonus = 0;
            }

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                Customer pro = ctx.Customers.Where(p => p.Code == model.Code).FirstOrDefault();
                pro.Name = model.Name;
                pro.Phone = model.Phone;
                pro.Address = model.Address;
                pro.Note = model.Note;
                pro.tentaisan = model.tentaisan;
                pro.loaigiayto = myViewModel.SelectedLoaiGiayTo;
                pro.Price = myViewModel.model.Price;

                if (pro.StartDate != model.StartDate)
                {
                    List<Loan> lstLoan = new List<Loan>();
                    pro.StartDate = model.StartDate;

                    List<Loan> lstTong = ctx.Loans.Where(p => p.IDCus == model.Code).ToList();
                    List<Loan> lstLoandadong = ctx.Loans.Where(p => p.IDCus == model.Code && p.Status == 1).ToList();
                    int sldadong = lstLoandadong.Count;

                    foreach (var item in lstTong)
                    {
                        ctx.Loans.Remove(item);
                    }

                    int day = model.songayno.HasValue ? (int)model.songayno : 1;
                    DateTime k = model.StartDate;

                    for (int i = 1; i <= 60; i++)
                    {
                        Loan temp = new Loan();
                        temp.Date = k.AddDays(day);
                        temp.IDCus = model.Code;
                        temp.Status = 0;
                        ctx.Loans.Add(temp);

                        k = temp.Date;
                        lstLoan.Add(temp);
                    }

                    for (int i = 0; i < sldadong; i++)
                    {
                        var temp = lstLoan.ElementAt(i);
                        temp.Status = 1;
                    }

                    ctx.SaveChanges();

                    ViewData["Loans"] = lstLoan;

                }

                ctx.SaveChanges();


            }

            if (myViewModel.fuMain != null && myViewModel.fuMain.ContentLength > 0)
            {
                string spDirPath = Server.MapPath("~/image");
                string targetDirPath = Path.Combine(spDirPath, myViewModel.model.Code.ToString());
                Directory.CreateDirectory(targetDirPath);

                string mainFileName = Path.Combine(targetDirPath, "main.jpg");
                //myViewModel.fuMain.SaveAs(mainFileName);

                Image bm = Image.FromStream(myViewModel.fuMain.InputStream);
                bm = Helper.Helper.ResizeBitmap((Bitmap)bm, 160, 160); /// new width, height
                bm.Save(Path.Combine(targetDirPath, "main.jpg"));
            }

            return RedirectToAction("LoadCustomerXE1", "Home");
        }

        public ActionResult Detail(string id)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                Customer model = ctx.Customers.FirstOrDefault(p => p.Code == id);
                List<Loan> list = ctx.Loans.Where(p => p.IDCus == id).ToList();
                ViewData["Loan"] = list;

                return View(model);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult GetCusDetail(string code)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                var codeList = ctx.Customers.Where(i => i.Code == code).ToList();

                var viewmodel = codeList.Select(x => new
                {
                    Code = x.Code,
                    Name = x.Name,
                    Phone = x.Phone,
                    StartDate = x.StartDate,
                    Address = x.Address,
                    Loan = x.Loan,
                    Price = x.Price,
                    tiengoc = x.tiengoc,
                    loaigiayto = x.loaigiayto,
                    Ghichu = x.Note,
                    OldCode = x.OldCode,
                    songayduocphepnothem = x.DayBonus
                });

                return Json(viewmodel);
            }
        }
        public ActionResult Addday(Loan model)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                ctx.Loans.Add(model);
                ctx.SaveChanges();
            }

            return RedirectToAction("LoadCustomer", "Home");
        }

        public static string timetemp;

        [HttpGet]
        public ActionResult UpdateLoan(int loanid, string songaydong, string idcus)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                decimal? ct = 0;
                int t = -1;
                int songay;
                decimal? amount = 0;
                decimal? remainingamount = 0;
                int? songaydatra;

                if (String.IsNullOrEmpty(songaydong))
                {
                    songay = 0;
                }
                else
                {
                    songay = Int32.Parse(songaydong);
                }

                if (songay == 0)
                {
                    Loan item = new Loan();


                    Customer csCustomer = new Customer();

                    item = ctx.Loans.Where(p => p.ID == loanid && p.IDCus == idcus).FirstOrDefault();

                    if (item.Type == true)
                    {
                        return Json(new { success = false, message = "Ngày đóng không hợp lệ" }, JsonRequestBehavior.AllowGet);
                    }

                    timetemp = item.Date.ToShortDateString();

                    item.Status = item.Status + 1;

                    if (item.Status >= 2)
                    {
                        item.Status = 0;
                    }

                    if (item.Status == 1)
                    {
                        csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();
                        List<Loan> lstSongaydatra = ctx.Loans.Where(p => p.IDCus == idcus && p.Status == 1 && p.Type == false).ToList();
                        songaydatra = lstSongaydatra.Count;

                        songaydatra++;
                        csCustomer.AmountPaid = songaydatra * csCustomer.Price;
                        csCustomer.RemainingAmount = csCustomer.Loan - csCustomer.AmountPaid;
                        t = 1;
                        csCustomer.DayPaids = songaydatra;

                        WriteHistory(csCustomer, 1, 0, loanid);
                    }
                    else
                    {
                        csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();
                        List<Loan> lstSongaydatra1 = ctx.Loans.Where(p => p.IDCus == csCustomer.Code && p.Status == 1 && p.Type == false).ToList();
                        songaydatra = lstSongaydatra1.Count;
                        songaydatra--;
                        csCustomer.AmountPaid = songaydatra * csCustomer.Price;
                        csCustomer.RemainingAmount = csCustomer.Loan - csCustomer.AmountPaid;
                        t = 0;
                        csCustomer.DayPaids = songaydatra;

                        WriteHistory(csCustomer, 0, 0, loanid);
                    }

                    ct = csCustomer.Price;
                    amount = csCustomer.AmountPaid ?? 0;
                    remainingamount = csCustomer.RemainingAmount ?? 0;

                    Helper.Helper.UpdateLoanCustomer(csCustomer);
                }
                else
                {
                    for (int i = 0; i < songay; i++)
                    {
                        Loan item = new Loan();

                        Customer csCustomer = new Customer();
                        item = ctx.Loans.Where(p => p.ID == loanid && p.IDCus == idcus).FirstOrDefault();
                        timetemp = item.Date.ToShortDateString();

                        if (item.Type == true)
                        {
                            return Json(new { success = false, message = "Ngày đóng không hợp lệ" }, JsonRequestBehavior.AllowGet);

                        }

                        loanid++;
                        item.Status = item.Status + 1;

                        if (item.Status >= 2)
                        {
                            item.Status = 0;
                        }

                        if (item.Status == 1)
                        {
                            csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();

                            List<Loan> lstSongaydatra2 = ctx.Loans.Where(p => p.IDCus == csCustomer.Code && p.Status == 1 && p.Type == false).ToList();
                            songaydatra = lstSongaydatra2.Count;
                            songaydatra++;
                            csCustomer.AmountPaid = songaydatra * csCustomer.Price;
                            csCustomer.RemainingAmount = csCustomer.Loan - csCustomer.AmountPaid;
                            t = 1;
                            csCustomer.DayPaids = songaydatra;

                            WriteHistory(csCustomer, 1, 0, loanid);
                            ctx.SaveChanges();
                        }
                        else
                        {
                            csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();
                            List<Loan> lstSongaydatra3 = ctx.Loans.Where(p => p.IDCus == csCustomer.Code && p.Status == 1 && p.Type == false).ToList();
                            songaydatra = lstSongaydatra3.Count;
                            songaydatra--;
                            csCustomer.AmountPaid = songaydatra * csCustomer.Price;
                            csCustomer.RemainingAmount = csCustomer.Loan - csCustomer.AmountPaid;
                            t = 0;
                            csCustomer.DayPaids = songaydatra;

                            WriteHistory(csCustomer, 0, 0, loanid);
                            ctx.SaveChanges();
                        }
                        ct = csCustomer.Price * songay;
                        amount = csCustomer.AmountPaid ?? 0;
                        remainingamount = csCustomer.RemainingAmount ?? 0;

                        Helper.Helper.UpdateLoanCustomer(csCustomer);
                    }
                }

                //var cs = ctx.Customers.Where(p => p.Code == idcus).FirstOrDefault();
                //List<Loan> lstSongaydatra4 = ctx.Loans.Where(p => p.IDCus == idcus && p.Status == 1).ToList();
                //int t1 = lstSongaydatra4.Count;
                //cs.DayPaids = t1;
                ctx.SaveChanges();

                return Json(new { success = true, oldval = loanid, status = t, songay = songay, amount = amount, remainingamount = remainingamount, ct = ct },
               JsonRequestBehavior.AllowGet);

            }
            //UpdateAllSongaydatra();
            //return View();
        }

        [HttpGet]
        public ActionResult UpdateNodung(int loanid, string songaydong, string idcus)
        {
            int t = -1;
            int songay;

            if (String.IsNullOrEmpty(songaydong))
                songay = 0;
            else
                songay = Int32.Parse(songaydong);

            if (songay == 0)
            {
                Loan item = new Loan();

                using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
                {
                    ctx.Configuration.ValidateOnSaveEnabled = false;
                    Customer csCustomer = new Customer();

                    item = ctx.Loans.Where(p => p.ID == loanid && p.IDCus == idcus).FirstOrDefault();
                    timetemp = item.Date.ToShortDateString();

                    item.Status = item.Status + 1;

                    if (item.Status >= 2)
                    {
                        item.Status = 0;
                    }

                    if (item.Status == 1)
                    {
                        csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();
                        t = 1;
                        WriteHistory(csCustomer, 1, 0, loanid);
                    }
                    else
                    {
                        csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();
                        t = 0;
                        WriteHistory(csCustomer, 0, 0, loanid);
                    }

                    ctx.SaveChanges();
                }
            }
            else
            {
                for (int i = 0; i < songay; i++)
                {
                    Loan item = new Loan();


                    using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
                    {
                        ctx.Configuration.ValidateOnSaveEnabled = false;
                        Customer csCustomer = new Customer();
                        item = ctx.Loans.Where(p => p.ID == loanid && p.IDCus == idcus).FirstOrDefault();
                        timetemp = item.Date.ToShortDateString();
                        item.Status = item.Status + 1;

                        if (item.Status >= 2)
                        {
                            item.Status = 0;
                        }

                        if (item.Status == 1)
                        {
                            csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();
                            t = 1;
                            WriteHistory(csCustomer, 1, 0, loanid);
                        }
                        else
                        {
                            csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();
                            t = 0;
                            WriteHistory(csCustomer, 0, 0, loanid);
                        }
                        loanid++;
                        ctx.SaveChanges();
                    }
                }
            }

            return Json(new { success = true, oldval = loanid, status = t, songay = songay },
                JsonRequestBehavior.AllowGet);
        }

        public ActionResult Reset(string id)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                Customer cs = ctx.Customers.FirstOrDefault(p => p.Code == id);
                //string code = cs.Code;
                //string name = cs.Name;
                //string phone = cs.Phone;
                //string address = cs.Address;

                //List<Loan> lstLoans = ctx.Loans.Where(p => p.IDCus == id).ToList();

                //ctx.Customers.Remove(cs);
                //{
                //    ctx.Loans.Remove(item);
                //}
                //ctx.SaveChanges();

                //foreach (var item in lstLoans)
                //Customer csCustomer = new Customer();
                //csCustomer.Code = code;
                //csCustomer.Name = name;
                //csCustomer.Phone = phone;
                //csCustomer.Address = address;
                //csCustomer.DayPaids = 0;
                //csCustomer.StartDate = DateTime.Now.Date;
                //ctx.Customers.Add(csCustomer);
                cs.Description = "End";
                timetemp = DateTime.Now.Date.ToShortDateString();
                WriteHistory(cs, -1, -1, -1);
                ctx.SaveChanges();
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TimKiemNoKhachHang()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && Helper.Helper.CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                //lst1 = ctx.GetCustomerEven1().ToList();
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHang1()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && Helper.Helper.CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                //lst1 = ctx.GetCustomerOdd1().ToList();

                foreach (Customer cs in lst1)
                {
                    cs.NgayNo = 0;
                    int count = 0;
                    int countMax = 0;

                    DateTime EndDate = DateTime.Now;

                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    Loan t1 = new Loan();

                    if (t.Count != 0)
                    {
                        t1 = t.First();
                    }

                    DateTime StartDate = t1.Date;

                    List<Loan> query = t.Where(p => p.Date >= StartDate && p.Date <= EndDate).ToList();

                    foreach (Loan temp in query)
                    {
                        if (temp.Status == 0)
                        {
                            count++;
                            countMax = count;
                        }
                        else
                        {
                            count = 0;
                        }
                    }

                    cs.NgayNo = countMax;
                    ctx.SaveChanges();
                }

                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangEvenXE1()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 12).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && Helper.Helper.CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangOddXE1()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 12).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && Helper.Helper.CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult LoadCustomerEven(int page = 1, int type = -1)
        {
            int pageSz = Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]);

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;

                List<Customer> lst = ctx.Customers
                    .Where(p => (type == -1 || p.type == type)).ToList();
                List<Customer> lst1 = new List<Customer>();

                int result;

                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }

                int count1 = lst1.Count();
                int nPages = count1 / pageSz + (count1 % pageSz > 0 ? 1 : 0);

                lst1 = lst1.OrderBy(p => p.CodeSort)
                    .Skip((page - 1) * pageSz)
                     .Take(pageSz).ToList();

                ViewBag.PageCount = nPages;
                ViewBag.CurPage = page;

                foreach (Customer cs in lst1)
                {
                    cs.NgayNo = 0;
                    int count = 0;
                    int countMax = 0;

                    DateTime EndDate = DateTime.Now;

                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    Loan t1 = new Loan();

                    if (t.Count != 0)
                    {
                        t1 = t.First();
                    }

                    DateTime StartDate = t1.Date;

                    List<Loan> query = t.Where(p => p.Date >= StartDate && p.Date <= EndDate).ToList();

                    foreach (Loan temp in query)
                    {
                        if (temp.Status == 0)
                        {
                            count++;
                            countMax = count;
                        }
                        else
                        {
                            count = 0;
                        }
                    }

                    cs.NgayNo = countMax;
                    ctx.SaveChanges();
                }

                return View(lst1);
            }
        }

        public ActionResult SearchName(string term)
        {
            var products = Helper.Helper.GetCustomer(term).Select(c => new { id = c.Code, value = c.Name });
            return Json(products, JsonRequestBehavior.AllowGet);
        }


        public JsonResult SearchCode(string term)
        {
            var products = Helper.Helper.GetCustomer1(term).Select(c => new { id = c.Code, value = c.Code });

            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchPhone(string term)
        {
            var products = Helper.Helper.GetCustomer2(term).Select(c => new { id = c.Code, value = c.Phone });
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchAddress(string term)
        {
            var products = Helper.Helper.GetCustomer3(term).Select(c => new { id = c.Code, value = c.Address });
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchTentaisan(string term)
        {
            var products = Helper.Helper.Gettentaisan(term).Select(c => new { id = c.Code, value = c.tentaisan });
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemoveItem(string proId)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                Customer ep = ctx.Customers.Where(p => p.Code == proId).FirstOrDefault();
                List<Loan> lstLoans = ctx.Loans.Where(p => p.IDCus == proId).ToList();

                ctx.Customers.Remove(ep);

                foreach (var item in lstLoans)
                {
                    ctx.Loans.Remove(item);
                }
                ctx.SaveChanges();
            }
            ViewBag.Delete = true;
            return RedirectToAction("LoadCustomer", "Home");
        }

        [HttpPost]
        public JsonResult DeleteCustomer(string id)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
                {
                    ctx.Configuration.ValidateOnSaveEnabled = false;
                    Customer cus = ctx.Customers.Where(o => o.Code == id).FirstOrDefault();
                    List<Loan> lstLoans = ctx.Loans.Where(p => p.IDCus == id).ToList();
                    if (cus != null)
                    {

                        Customer temp = new Customer();
                        temp.Name = cus.Name;
                        temp.Phone = cus.Phone;
                        temp.Address = cus.Address;
                        temp.Description = cus.Description;
                        temp.type = cus.type;
                        temp.StartDate = cus.StartDate;
                        temp.IsDeleted = true;
                        temp.Code = Helper.Helper.RandomString(4);
                        temp.Loan = cus.Loan;
                        temp.tiengoc = cus.tiengoc;
                        temp.OldCode = cus.Code;
                        ctx.Customers.Add(temp);
                        foreach (var item in lstLoans)
                        {
                            ctx.Loans.Remove(item);
                        }

                        ctx.Customers.Remove(cus);
                        ctx.SaveChanges();
                        result["status"] = "success";
                    }
                }
                //string path = Server.MapPath("~/data.txt");
                //using (StreamWriter sw = (System.IO.File.Exists(path)) ? System.IO.File.AppendText(path)
                //    : System.IO.File.CreateText(path))
                //{
                //    Helper.Helper.Log("Test1", sw);
                //}
            }
            catch (Exception ex)
            {
                result["status"] = "error";
                result["message"] = ex.Message;
            }

            return Json(result);
        }

        [HttpPost]
        public JsonResult AddLoan(CustomLoan l)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                //string t = l.Money.Replace(',', ' ');
                //t = t.ToCharArray()
                // .Where(c => !Char.IsWhiteSpace(c))
                // .Select(c => c.ToString())
                // .Aggregate((a, b) => a + b);

                //int i = t.IndexOf('.');
                //t = t.Remove(i);

                int money = Int32.Parse(l.Money);

                Customer cs = ctx.Customers.Where(p => p.Code == l.IDCus).FirstOrDefault();

                Loan model = new Loan();
                model.Date = l.Date;
                model.Status = 1;
                model.IDCus = l.IDCus;
                model.Type = true;
                model.money = money;

                ctx.Loans.Add(model);

                //if (cs != null)
                //{
                //    cs.AmountPaid += money;
                //    cs.RemainingAmount = cs.Loan - cs.AmountPaid;
                //}

                timetemp = DateTime.Now.ToString();
                ctx.SaveChanges();
                WriteHistory(cs, 1, money, model.ID);

                return Json(new { amountpaid = cs.AmountPaid, remainingamount = cs.RemainingAmount },
                    JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SumMoneyByCode(int type, DateTime datetime)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                try
                {

                    //DateTime dateflag1 = DateTime.Parse(dateflag);
                    datetime = datetime.Date;
                    StringBuilder str = new StringBuilder();
                    //List<SumMoneyByCode_Result> a = ctx.SumMoneyByCode(datetime, type).ToList();

                    var histories = ctx.histories
                        .Where(p => p.Ngaydongtien.Value.Year == datetime.Year
                                    && p.Ngaydongtien.Value.Month == datetime.Month
                                    && p.Ngaydongtien.Value.Day == datetime.Day
                                    && (p.Customer.type == type || type == -1 || type == -2)
                                    && p.Customer.StartDate >= dtcompareNewCs
                                   ).ToList();
                    var grouped = histories.GroupBy(p => p.CustomerId);
                    var historyFilterd = new List<history>();

                    var groupdId = grouped.Select(g => g.OrderByDescending(r => r.ID).FirstOrDefault());
                    //var historyTrue = ctx.histories
                    //    .Where(p => p.Ngaydongtien.Value.Year == datetime.Year
                    //                && p.Ngaydongtien.Value.Month == datetime.Month
                    //                && p.Ngaydongtien.Value.Day == datetime.Day
                    //                && (p.Customer.type == type || type == -1 || type == -2) 
                    //                && p.status == 1 && p.Customer.StartDate >= dtcompareNewCs
                    //               ).Select(t => t.price).Sum();


                    //var historyFalse = ctx.histories
                    //    .Where(p => p.Ngaydongtien.Value.Year == datetime.Year
                    //                && p.Ngaydongtien.Value.Month == datetime.Month
                    //                && p.Ngaydongtien.Value.Day == datetime.Day
                    //                && (p.Customer.type == type || type == -1 || type == -2) && p.status == 0 && p.Customer.StartDate >= dtcompareNewCs
                    //                ).Select(t => t.price).Sum();


                    //var sum = (historyTrue.HasValue ? historyTrue.Value : 0) - (historyFalse.HasValue ? historyFalse.Value : 0);
                    //foreach (var item in a)
                    //    str.Append(item.subcode + " : " + (item.sumval ?? 0) + "; ");
                    //string sumval = a.Sum(p => p.sumval).ToString();
                    decimal? sum = 0;

                    foreach (var item in groupdId)
                    {
                        if (item.status == 1)
                        {
                            sum += item.price;
                        }

                    }
                    if (sum != 0)
                    {
                        str.Append("Tổng:" + sum.ToString());
                        TempData["message"] = str.ToString();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }


            }

            return Json(type);
        }

        public ActionResult ResetDatetime(int type, string message, DateTime datetime)
        {

            //HttpCookie cookie = Request.Cookies["dateflag"];

            //if (cookie == null)
            //{
            //    // no cookie found, create it
            //    cookie = new HttpCookie("dateflag");
            //    cookie.Values["datetimeinput"] = DateTime.Now.ToString();

            //}
            //else
            //{
            //    // update the cookie values

            //    cookie.Values["datetimeinput"] = DateTime.Now.ToString();
            //}
            //cookie.Expires = DateTime.UtcNow.AddDays(30);
            //Response.Cookies.Add(cookie);

            if (!string.IsNullOrEmpty(message))
            {
                using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
                {
                    Message msg = new Message();
                    msg.Message1 = message;
                    msg.Date = datetime;
                    msg.type = 1;
                    ctx.Messages.Add(msg);
                    ctx.SaveChanges();
                }
            }

            //ViewBag.startdate = DateTime.Now.ToString();
            return Json(type);
        }
        public void WriteHistory(Customer p, int type, int money, int loanid)
        {
            StringBuilder str = new StringBuilder();
            history hs = new history();

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                if (type == 1) // dong tien
                {
                    str.Append("Đóng tiền ngày: " + timetemp);
                    hs.CustomerId = p.Code;
                    hs.Detail = str.ToString();
                    hs.Ngaydongtien = DateTime.Now;
                    hs.price = money == 0 ? p.Price : money;
                    hs.status = 1;
                    hs.loanid = loanid;
                    ctx.histories.Add(hs);
                }
                else if (type == 0) // ko dong tien
                {
                    str.Append("Xóa đóng tiền ngày: " + timetemp);
                    hs.CustomerId = p.Code;
                    hs.Detail = str.ToString();
                    hs.Ngaydongtien = DateTime.Now;
                    hs.price = money == 0 ? p.Price : money;
                    hs.status = 0;
                    hs.loanid = loanid;
                    ctx.histories.Add(hs);
                }
                else
                {
                    str.Append("Kết thúc dây nợ ngày : " + timetemp);
                    hs.CustomerId = p.Code;
                    hs.Detail = str.ToString();
                    hs.Ngaydongtien = DateTime.Now;
                    hs.price = money == 0 ? p.Price : money;
                    hs.status = 0;
                    hs.loanid = -1;
                    ctx.histories.Add(hs);
                }


                ctx.SaveChanges();
            }
        }

    }
}