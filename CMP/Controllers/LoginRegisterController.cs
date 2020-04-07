using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CMP.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CMP.Controllers
{
    public class LoginRegisterController : Controller
    {

        private readonly IConfiguration _configuration;

        public LoginRegisterController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registar(LoginRegistarView dados)
        {
            if (ModelState.IsValid)
            {
                bool valid = true;
                int id = -1;

                if (!verificarUsername(dados.username))
                {
                    ModelState.AddModelError("username", "Username já existe");
                    return View("Index");
                }

                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"Insert Into Account (username, email, password, newsletter) Values ('{dados.username}','{dados.email}','{dados.password}','false')";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = System.Data.CommandType.Text;
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }

                    sql = $"SELECT * FROM Account WHERE id = (SELECT MAX(id) FROM Account)";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                id = Convert.ToInt32(dataReader["id"]);
                            }
                        }
                        connection.Close();
                    }

                    sql = $"Insert Into Cliente (nome, morada, account_id, role_id,group_id) Values ('{dados.nome}','{dados.morada}','{id}','{1}','{1}')"; //INSERE SEMPRE DO TIPO USER
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = System.Data.CommandType.Text;
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View("Index");
            }

        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public bool verificarUsername(String user)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Account";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (Convert.ToString(dataReader["username"]).Equals(user))
                            {
                                return false;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return true;
        }
    }
  
}
