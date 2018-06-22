using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using cbeltworkpls.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace csharpbelt.Controllers
{
    public class HomeController : Controller
    {
        private cbeltworkplscontext _context;

        public HomeController(cbeltworkplscontext context) {
            _context = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("register")]
        public IActionResult Register(users newuser, string confpassword) {

            if(newuser.first_name == null || newuser.last_name == null || newuser.password == null || confpassword == null || newuser.email == null) {
                return RedirectToAction("Index");
            }

            if(confpassword != newuser.password) {
                return RedirectToAction("Index");
            }

            if(_context.users.SingleOrDefault(user => user.email == newuser.email) != null) {
                return RedirectToAction("Index");
            }

            if(!(Regex.IsMatch(newuser.first_name, @"^[a-zA-Z]+$")) || !(Regex.IsMatch(newuser.last_name, @"^[a-zA-Z]+$"))) {
                return RedirectToAction("Index");
            }

            if(!(Regex.IsMatch(newuser.password, @"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[!@#\$%\^&\*])"))) {
                return RedirectToAction("Index");
            }

            if(ModelState.IsValid) {
                if(HttpContext.Session.GetInt32("userid") == null) {
                    HttpContext.Session.SetInt32("userid", -1);
                }
                PasswordHasher<users> Hasher = new PasswordHasher<users>();
                newuser.password = Hasher.HashPassword(newuser, newuser.password);
                _context.Add(newuser);
                _context.SaveChanges();
                users ReturnedValue = _context.users.SingleOrDefault(user => user.email == newuser.email);
                HttpContext.Session.SetInt32("userid", ReturnedValue.idusers);
                int? temp = HttpContext.Session.GetInt32("userid");
                int truetemp = (int)temp;
                return RedirectToAction("Home");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Loginsub(string email, string password) {
            if(HttpContext.Session.GetInt32("userid") == null) {
                HttpContext.Session.SetInt32("userid", -1);
            }
            if(email == null || password == null) {
                return RedirectToAction("Index");
            }
            users ReturnedValue = _context.users.SingleOrDefault(user => user.email == email);
            if(ReturnedValue != null && password != null) {
                HttpContext.Session.SetInt32("userid", ReturnedValue.idusers);
                var Hasher = new PasswordHasher<users>();
                if(0 != Hasher.VerifyHashedPassword(ReturnedValue, ReturnedValue.password, password))
                {
                    int? temp = HttpContext.Session.GetInt32("userid");
                    int truetemp = (int)temp;
                    return RedirectToAction("Home");
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Home")]
        public IActionResult Home() {
            if(HttpContext.Session.GetInt32("userid") == null) {
                return RedirectToAction("Index");
            }
            int? temp = HttpContext.Session.GetInt32("userid");
            int truetemp = (int)temp;
            users ReturnedValue = _context.users.SingleOrDefault(user => user.idusers == truetemp);
            ViewBag.Name = ReturnedValue.first_name;
            ViewBag.Userid = truetemp;

            DateTime now = DateTime.Now;

            List<activities> allact = _context.activities.Where(a => a.date > now).Include(act => act.participants).ThenInclude(f => f.user).ToList();
            List<activities> sorted = allact.OrderBy(o => o.date).ToList();
            ViewBag.Allact = sorted;
            List<participants> allparts = _context.participants.ToList();
            ViewBag.Allparts = allparts;

            
            return View();
        }

        [HttpGet]
        [Route("new")]
        public IActionResult New() {
            if(HttpContext.Session.GetInt32("userid") == null) {
                return RedirectToAction("Index");
            }
            ViewBag.Time = DateTime.Now.ToString("yyyy'-'MM'-'dd");
            return View();
        }

        [HttpPost]
        [Route("subact")]
        public IActionResult Subact(DateTime date, string actname, string durationtype, int durationint, string description) {
            int? temp = HttpContext.Session.GetInt32("userid");
            int truetemp = (int)temp;
            users ReturnedValue = _context.users.SingleOrDefault(user => user.idusers == truetemp);

            DateTime now = DateTime.Now;
            if(date <= now) {
                return RedirectToAction("New");
            }

            List<participants> youracts = _context.participants.Where(p => p.idusers == truetemp).Include(p => p.activity).ToList();
            foreach(var activity in youracts) {
                if(activity.activity.durationtype == "minutes") {
                    System.TimeSpan duration = new System.TimeSpan(0, 0, activity.activity.durationint, 0);
                    DateTime actend = activity.activity.date.Add(duration);

                    if(durationtype == "minutes") {
                        System.TimeSpan duration2 = new System.TimeSpan(0, 0, durationint, 0);
                        DateTime thisactend = date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("New");
                        }
                    }
                    else if(durationtype == "hours") {
                        System.TimeSpan duration2 = new System.TimeSpan(0, durationint, 0, 0);
                        DateTime thisactend = date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("New");
                        }
                    }
                    else if(durationtype == "days") {
                        System.TimeSpan duration2 = new System.TimeSpan(durationint, 0, 0, 0);
                        DateTime thisactend = date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("New");
                        }
                    }
                    if(date > activity.activity.date && date < actend) {
                        return RedirectToAction("New");
                    }
                } else if(activity.activity.durationtype == "hours") {
                    System.TimeSpan duration = new System.TimeSpan(0, activity.activity.durationint, 0, 0);
                    DateTime actend = activity.activity.date.Add(duration);

                    if(durationtype == "minutes") {
                        System.TimeSpan duration2 = new System.TimeSpan(0, 0, durationint, 0);
                        DateTime thisactend = date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("New");
                        }
                    }
                    else if(durationtype == "hours") {
                        System.TimeSpan duration2 = new System.TimeSpan(0, durationint, 0, 0);
                        DateTime thisactend = date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("New");
                        }
                    }
                    else if(durationtype == "days") {
                        System.TimeSpan duration2 = new System.TimeSpan(durationint, 0, 0, 0);
                        DateTime thisactend = date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("New");
                        }
                    }
                    if(date > activity.activity.date && date < actend) {
                        return RedirectToAction("New");
                    }
                } else if(activity.activity.durationtype == "days") {
                    System.TimeSpan duration = new System.TimeSpan(activity.activity.durationint, 0, 0, 0);
                    DateTime actend = activity.activity.date.Add(duration);

                    if(durationtype == "minutes") {
                        System.TimeSpan duration2 = new System.TimeSpan(0, 0, durationint, 0);
                        DateTime thisactend = date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("New");
                        }
                    }
                    else if(durationtype == "hours") {
                        System.TimeSpan duration2 = new System.TimeSpan(0, durationint, 0, 0);
                        DateTime thisactend = date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("New");
                        }
                    }
                    else if(durationtype == "days") {
                        System.TimeSpan duration2 = new System.TimeSpan(durationint, 0, 0, 0);
                        DateTime thisactend = date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("New");
                        }
                    }
                    if(date > activity.activity.date && date < actend) {
                        return RedirectToAction("New");
                    }
                }
            }

            activities newact = new activities {
                actname = actname,
                date = date,
                durationint = durationint,
                durationtype = durationtype,
                coordname = ReturnedValue.first_name,
                coordid = truetemp,
                description = description
            };
            _context.Add(newact);
            _context.SaveChanges();

            int check = newact.idactivities;

            participants newpart = new participants {
                idusers = truetemp,
                idactivities = check
            };
            _context.Add(newpart);
            _context.SaveChanges();

            return RedirectToAction("Activityview", new { id = check });
        }

        [HttpGet]
        [Route("activity/{id}")]
        public IActionResult Activityview(int id) {
            if(HttpContext.Session.GetInt32("userid") == null) {
                return RedirectToAction("Index");
            }
            int? temp = HttpContext.Session.GetInt32("userid");
            int truetemp = (int)temp;
            ViewBag.Userid = truetemp;
            activities ReturnedValue = _context.activities.SingleOrDefault(act => act.idactivities == id);
            List<participants> allparts = _context.participants.Where(part => part.idactivities == id && part.idusers != ReturnedValue.coordid).Include(u => u.user).ToList();
            ViewBag.Activity = ReturnedValue;
            ViewBag.Guests = allparts;
            ViewBag.Desc = ReturnedValue.description;

            return View();
        }

        [HttpGet]
        [Route("/delete/{id}")]
        public IActionResult Delete(int id) {
            if(HttpContext.Session.GetInt32("userid") == null) {
                return RedirectToAction("Index");
            }
            activities toremove = _context.activities.SingleOrDefault(sec => sec.idactivities == id);
            _context.activities.Remove(toremove);
            _context.SaveChanges();
            return RedirectToAction("Home");
        }

        [HttpGet]
        [Route("/join/{id}")]
        public IActionResult Join(int id) {
            if(HttpContext.Session.GetInt32("userid") == null) {
                return RedirectToAction("Index");
            }
            int? temp = HttpContext.Session.GetInt32("userid");
            int truetemp = (int)temp;

            activities activ = _context.activities.SingleOrDefault(a => a.idactivities == id);


            List<participants> youracts = _context.participants.Where(p => p.idusers == truetemp).Include(p => p.activity).ToList();
            foreach(var activity in youracts) {
                if(activity.activity.durationtype == "minutes") {
                    System.TimeSpan duration = new System.TimeSpan(0, 0, activity.activity.durationint, 0);
                    DateTime actend = activity.activity.date.Add(duration);

                    if(activ.durationtype == "minutes") {
                        System.TimeSpan duration2 = new System.TimeSpan(0, 0, activ.durationint, 0);
                        DateTime thisactend = activ.date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("Home");
                        }
                    }
                    else if(activ.durationtype == "hours") {
                        System.TimeSpan duration2 = new System.TimeSpan(0, activ.durationint, 0, 0);
                        DateTime thisactend = activ.date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("Home");
                        }
                    }
                    else if(activ.durationtype == "days") {
                        System.TimeSpan duration2 = new System.TimeSpan(activ.durationint, 0, 0, 0);
                        DateTime thisactend = activ.date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("Home");
                        }
                    }
                    if(activ.date > activity.activity.date && activ.date < actend) {
                        return RedirectToAction("Home");
                    }
                } else if(activity.activity.durationtype == "hours") {
                    System.TimeSpan duration = new System.TimeSpan(0, activity.activity.durationint, 0, 0);
                    DateTime actend = activity.activity.date.Add(duration);

                    if(activ.durationtype == "minutes") {
                        System.TimeSpan duration2 = new System.TimeSpan(0, 0, activ.durationint, 0);
                        DateTime thisactend = activ.date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("Home");
                        }
                    }
                    else if(activ.durationtype == "hours") {
                        System.TimeSpan duration2 = new System.TimeSpan(0, activ.durationint, 0, 0);
                        DateTime thisactend = activ.date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("Home");
                        }
                    }
                    else if(activ.durationtype == "days") {
                        System.TimeSpan duration2 = new System.TimeSpan(activ.durationint, 0, 0, 0);
                        DateTime thisactend = activ.date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("Home");
                        }
                    }
                    if(activ.date > activity.activity.date && activ.date < actend) {
                        return RedirectToAction("Home");
                    }
                } else if(activity.activity.durationtype == "days") {
                    System.TimeSpan duration = new System.TimeSpan(activity.activity.durationint, 0, 0, 0);
                    DateTime actend = activity.activity.date.Add(duration);

                    if(activ.durationtype == "minutes") {
                        System.TimeSpan duration2 = new System.TimeSpan(0, 0, activ.durationint, 0);
                        DateTime thisactend = activ.date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("Home");
                        }
                    }
                    else if(activ.durationtype == "hours") {
                        System.TimeSpan duration2 = new System.TimeSpan(0, activ.durationint, 0, 0);
                        DateTime thisactend = activ.date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("Home");
                        }
                    }
                    else if(activ.durationtype == "days") {
                        System.TimeSpan duration2 = new System.TimeSpan(activ.durationint, 0, 0, 0);
                        DateTime thisactend = activ.date.Add(duration2);
                        if(thisactend > activity.activity.date && thisactend < actend) {
                            return RedirectToAction("Home");
                        }
                    }
                    if(activ.date > activity.activity.date && activ.date < actend) {
                        return RedirectToAction("Home");
                    }
                }
            }

            participants newpart = new participants {
                idusers = truetemp,
                idactivities = id
            };
            _context.Add(newpart);
            _context.SaveChanges();
            return RedirectToAction("Home");
        }

        [HttpGet]
        [Route("/leave/{id}")]
        public IActionResult Leave(int id) {
            if(HttpContext.Session.GetInt32("userid") == null) {
                return RedirectToAction("Index");
            }
            int? temp = HttpContext.Session.GetInt32("userid");
            int truetemp = (int)temp;
            participants toremove = _context.participants.SingleOrDefault(sec => sec.idactivities == id && sec.idusers == truetemp);
            _context.participants.Remove(toremove);
            _context.SaveChanges();
            return RedirectToAction("Home");
        }

        [HttpGet]
        [Route("logout")]
        public IActionResult Logout() {
            int? temp = HttpContext.Session.GetInt32("userid");
            int truetemp = (int)temp;
            HttpContext.Session.Clear();
            int? temp2 = HttpContext.Session.GetInt32("userid");
            int truetemp2 = (int)temp;
            return RedirectToAction("Index");
        }
    

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
