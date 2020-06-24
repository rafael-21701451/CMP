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
            List<ProjetoAtual> projetosAtuais = new List<ProjetoAtual>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Produtor_Projeto WHERE produtor_id={this.User.Claims.ElementAt(2).Value}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            ProjetoAtual pa = new ProjetoAtual();
                            pa.id = getIdProjeto(Convert.ToInt32(dataReader["projeto_id"]));
                            int briefingID = getIdBriefing(pa.id);
                            int pcID = getPCByBriefingID(briefingID);
                            pa.produto = getProductByPCID(pcID);
                            pa.atribuidor = getNomeCMByID(Convert.ToInt32(dataReader["content_manager_id"]));
                            if (!getEstadoProjeto(pa.id))
                            {
                                projetosAtuais.Add(pa);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return View(projetosAtuais);
        }

        public int getIdProjeto(int idProjeto)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Projeto WHERE id={idProjeto}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            return Convert.ToInt32(dataReader["id"]);

                        }
                    }
                    connection.Close();
                }
            }
            return -1;
        }

        public int getIdBriefing(int idProjeto)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Projeto WHERE id={idProjeto}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            return Convert.ToInt32(dataReader["id_briefing"]);

                        }
                    }
                    connection.Close();
                }
            }
            return -1;
        }

        public Boolean getEstadoProjeto(int idProjeto)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Projeto WHERE id={idProjeto}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            return Convert.ToBoolean(dataReader["versao_final"]);

                        }
                    }
                    connection.Close();
                }
            }
            return false;
        }

        public int getPCByBriefingID(int idBriefing)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Briefing WHERE id = {idBriefing}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            return Convert.ToInt32(dataReader["produto_compra_id"]);
                        }
                    }
                    connection.Close();
                }
            }
            return -1;
        }

        public String getProductByPCID(int pcid)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Produto_Compra WHERE id = {pcid}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            return getProductByID(Convert.ToInt32(dataReader["produto_id"]));
                        }
                    }
                    connection.Close();
                }
            }
            return "";

        }

        public String getProductByID(int pID)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Produto";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (Convert.ToInt32(dataReader["id"]) == pID)
                            {
                                return Convert.ToString(dataReader["nome"]);

                            }
                        }
                    }
                    connection.Close();
                }
            }
            return "";
        }

        public String getNomeCMByID(int idCM)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Content_Manager WHERE id={idCM}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            return Convert.ToString(dataReader["nome"]);
                        }
                    }
                    connection.Close();
                }
            }
            return "";
        }
    }
}
