using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testIdentity.Models;

namespace testIdentity.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Password([FromQuery] string pw)
        {
            pw = Cryptography.EncryptBySHA1(pw);
            return Ok(new {result = pw});
        }
    }
}