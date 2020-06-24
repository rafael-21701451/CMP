using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CMP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Security.Policy;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http2;

namespace CMP.Controllers
{
    [Authorize]
    public class AreaProdutorController : Controller
    {

        private readonly IConfiguration _configuration;

        public AreaProdutorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
           
            return View();
        }

        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ProjetosAtuais()
        {
            return View();
        }
    }
}
