using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CMP.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
        public async Task<IActionResult> Login(LoginRegistarView dados)
        {


            bool valid = true;
            int id = -1;
            String email = dados.email;
            String password = dados.password;

            if (String.IsNullOrEmpty(email))
            {
                valid = false;
                ModelState.AddModelError("email", "Email obrigatório");
            }
            if (String.IsNullOrEmpty(password))
            {
                valid = false;
                ModelState.AddModelError("password", "Password obrigatória");
            }
            if (valid == false)
            {
                return View("Index");
            }
            if (!verificarConta(email, password))
            {
                ModelState.AddModelError("password", "Email e/ou password incorreta");
                return View("Index");
            }

            var claims = new[] {
                                new Claim(ClaimTypes.Name, email),//0
                                new Claim("nome", getNomeByEmail(email)),//1
                                new Claim("id", Convert.ToString(getIdByEmail(email))),//2
                            };

            if (!dados.sessao)
            {
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false,
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),authProperties);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),authProperties);
                return RedirectToAction("Index", "Home");
            }
            
        }

        [HttpPost]
        public IActionResult Registar(LoginRegistarView dados)
        {
           
                bool valid = true;
                int id = -1;

                if (String.IsNullOrEmpty(dados.nome))
                {
                    valid = false;
                    ModelState.AddModelError("nome", "Nome obrigatório");
                }
                if (String.IsNullOrEmpty(dados.morada))
                {
                    valid = false;
                    ModelState.AddModelError("morada", "Morada obrigatória");
                }
                if (String.IsNullOrEmpty(dados.username))
                {
                    valid = false;
                    ModelState.AddModelError("username", "Username obrigatório");
                }
                if (String.IsNullOrEmpty(dados.emailReg))
                {
                    valid = false;
                    ModelState.AddModelError("emailReg", "Email obrigatório");
                }
                if (String.IsNullOrEmpty(dados.passwordReg))
                {
                    valid = false;
                    ModelState.AddModelError("passwordReg", "Password obrigatória");
                }
            if (!dados.termos)
            {
                valid = false;
                ModelState.AddModelError("termos", "É necessário aceitar os termos");
            }
            if (valid == false)
            {
                return View("Index");
            }
            if (!verificarUsername(dados.username))
                {
                    valid = false;
                    ModelState.AddModelError("username", "Username já existe");
                }
                if (!verificarEmail(dados.emailReg))
                {
                    valid = false;
                    ModelState.AddModelError("emailReg", "Email já existe");
                }
                if (valid == false)
                {
                    return View("Index");
                }

                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"Insert Into Account (username, email, password, newsletter) Values ('{dados.username}','{dados.emailReg}','{dados.passwordReg}','false')";
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

        public bool verificarEmail(String email)
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
                            if (Convert.ToString(dataReader["email"]).Equals(email))
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

        public bool verificarConta(String email, String password)
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
                            if (Convert.ToString(dataReader["email"]).Equals(email) && Convert.ToString(dataReader["password"]).Equals(password))
                            {
                                return true;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return false;
        }

        public String getNomeByEmail(String email)
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
                            if (Convert.ToString(dataReader["email"]).Equals(email))
                            {
                                return getNomeCliente(Convert.ToInt32(dataReader["id"]));
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return null;
        }

        public int getIdByEmail(String email)
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
                            if (Convert.ToString(dataReader["email"]).Equals(email))
                            {
                                return Convert.ToInt32(dataReader["id"]);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return -1;
        }

        public String getNomeCliente(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Cliente";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (Convert.ToInt32(dataReader["account_id"])==id)
                            {
                                return Convert.ToString(dataReader["nome"]);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return null;

        }
    }
  
}
