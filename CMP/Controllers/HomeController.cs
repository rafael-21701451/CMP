using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CMP.Models;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace CMP.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Contactos()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contactos(ContactosView dadosContacto)
        {
            if (ModelState.IsValid)
            {
                return View("Index");
            }
            else
            {
                return View();
            }
        }



        public IActionResult Index()
        {
            if(@User.Identity.IsAuthenticated == false)
            {
                return View();

            }
            else
            {
                if (this.User.Claims.ElementAt(3).Value.Equals("Content Manager"))
                {
                    return RedirectToAction("Index", "AreaCM");
                }
                else if (this.User.Claims.ElementAt(3).Value.Equals("Produtor"))
                {
                    return RedirectToAction("Index", "AreaProdutor");
                }
                else
                {
                    return View();
                }
            }
            
        }

        
        [HttpPost]
        public IActionResult Index(Account account)
        {
            String email = "";
            String nomeUser = "";
            int id = -1;
            bool jaSubscrito = false;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Select * From Account Where email='{account.email}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            id = Convert.ToInt32(dataReader["id"]);
                            email = Convert.ToString(dataReader["email"]);
                           jaSubscrito = Convert.ToBoolean(dataReader["newsletter"]);
                        }
                    }
                    connection.Close();
                }

                sql = $"Select * From Cliente Where account_id='{id}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            nomeUser = Convert.ToString(dataReader["nome"]);
                        }
                    }
                    connection.Close();
                }
            }
            if (String.IsNullOrEmpty(account.email))
            {
                ModelState.AddModelError("email", "Email Obrigatório");
                return View();
            }
            else if (jaSubscrito || email.Equals(""))
            {
                return View();
            }
            else
            {
                SmtpClient client = new SmtpClient("smtp.live.com");
                client.EnableSsl = true;
                client.UseDefaultCredentials = true;
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("rafael_lemos@live.com.pt");
                mailMessage.To.Add(email);
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = "<html>"
                                  + "<body>"
                                  + "<div>"
                                  + $"<p>Olá {nomeUser},</p>"
                                  + "</div>"
                                  + "<div>"
                                  + "<p>Obrigado por subscrever à nossa newsletter.</p>"
                                  + "</div>"
                                  + "<div>"
                                  + $"<p>Irá recber todas as novidades mais recentes da plataforma!</p>"
                                  + "</div>"
                                  + "</body>"
                                  + "</html>";
                mailMessage.Subject = "Subscrição Newsletter";
                client.Send(mailMessage);
                return View();
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
