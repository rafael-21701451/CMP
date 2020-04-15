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

namespace CMP.Controllers
{
    public class AreaClienteController : Controller
    {

        private readonly IConfiguration _configuration;

        public AreaClienteController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MinhasEncomendas()
        {
            List<CompraPerfil> compras = new List<CompraPerfil>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Compra";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            CompraPerfil compra = new CompraPerfil();
                            compra.id = Convert.ToInt32(dataReader["id"]);
                            compra.data = getDataCompra( Convert.ToInt32(dataReader["id"]));
                            compra.total = Convert.ToInt32(dataReader["total"]);
                            compra.estado = getNomeEstado(Convert.ToInt32(dataReader["estado_id"]));
                            if (!compra.estado.Equals("Por Pagar"))
                            {
                                compras.Add(compra);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return View(compras);
        }

        public string getNomeEstado(int idEstado)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Estado";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (Convert.ToInt32(dataReader["id"]) == idEstado)
                            {
                                return Convert.ToString(dataReader["estado"]);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return "";
        }

        public string getDataCompra(int idCompra)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Fatura";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (Convert.ToInt32(dataReader["compra_id"]) == idCompra)
                            {
                                return string.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(dataReader["data"]));
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return "";
        }

        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index","Home");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
