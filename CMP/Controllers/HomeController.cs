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

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(Account account)
        {
            String email = "";
            String nomeUser = "";
            bool jaSubscrito = false;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Select * From AppLogin Where Email='{account.email}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            email = Convert.ToString(dataReader["Email"]);
                            nomeUser = Convert.ToString(dataReader["Login"]);
                           //jaSubscrito = Convert.ToBoolean(dataReader["subscricao"]);
                        }
                    }
                    connection.Close();
                }
            }
            if (email.Equals(""))
            {
                ModelState.AddModelError("", "Email Obrigatório");
                return View();

            }
            else if (jaSubscrito)
            {
                return View();
            }
            else
            {
                SmtpClient client = new SmtpClient("smtp.live.com");
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("rafael_lemos@live.com.pt", "Sporting7675.");
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
