﻿using System;
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
    public class AreaCMController : Controller
    {

        private readonly IConfiguration _configuration;

        public AreaCMController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

      

        public IActionResult Index()
        {    
            return View();
        }
        public IActionResult Produtores()
        {
            List<Produtor> produtores = new List<Produtor>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Produtor";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            Produtor p = new Produtor();
                            p.id = Convert.ToInt32(dataReader["id"]);
                            p.especialidade = Convert.ToString(dataReader["area"]);
                            p.produtor = Convert.ToString(dataReader["nome"]);
                            p.projetosAtuais = getProjetosAtuais(p.id);
                            produtores.Add(p);
                        }
                    }
                    connection.Close();
                }
            }
            return View(produtores);
        }

        public IActionResult ConfirmarProdutor(int idProdutor)
        {
            Produtor produtor = new Produtor();
            return View(produtor);
        }

        public int getProjetosAtuais(int idProdutor)
        {
            int numProjetos = 0;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Produtor_Projeto WHERE produtor_id={idProdutor}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            numProjetos = verificarSeProjetoJaTerminou(Convert.ToInt32(dataReader["projeto_id"]));
                        }
                    }
                    connection.Close();
                }
            }
            return numProjetos;
        }

        public int verificarSeProjetoJaTerminou(int projetoid)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Projeto WHERE id={projetoid}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (!Convert.ToBoolean(dataReader["versao_final"]))
                            {
                                return 1;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return 0;
        }

        public IActionResult ProjetosPorAtribuir()
        {
            List <ProjetoPorAtribuir> projetosPorAtribuir = new List<ProjetoPorAtribuir>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Projeto WHERE versao = 0";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            ProjetoPorAtribuir ppa = new ProjetoPorAtribuir();
                            ppa.id = Convert.ToInt32(dataReader["id"]);
                            int pcID = getPCByBriefingID(Convert.ToInt32(dataReader["id_briefing"]));
                            ppa.produto = getProductByPCID(pcID);
                            ppa.comprador = getCompradorByPCID(pcID);
                            ppa.idCompra = getCompraByPCID(pcID);
                            projetosPorAtribuir.Add(ppa);
                        }
                    }
                    connection.Close();
                }
            }
            return View(projetosPorAtribuir);
        }

        public IActionResult VerProjetoPorAtribuir(int idProjeto)
        {
            ProjetoView ppa = new ProjetoView();
            ppa.projetoID = idProjeto;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Projeto WHERE id = {idProjeto}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            int pcID = getPCByBriefingID(Convert.ToInt32(dataReader["id_briefing"]));
                            ppa.descProduto = getProductByPCID(pcID);
                            ppa.produto = getProductCategoryByPCID(pcID);
                        }
                    }
                    connection.Close();
                }
            }
            return View(ppa);
        }

        public String getProductCategoryByPCID(int pcid)
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
                            return getProductCategoryByID(Convert.ToInt32(dataReader["produto_id"]));
                        }
                    }
                    connection.Close();
                }
            }
            return "";

        }

        public String getProductCategoryByID(int pID)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Produto WHERE id={pID}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            
                                return getProductCategory(Convert.ToInt32(dataReader["categoria_id"]));
                            
                        }
                    }
                    connection.Close();
                }
            }
            return "";
        }

        public String getProductCategory(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Categoria";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (Convert.ToInt32(dataReader["id"]) == id)
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

        public String getCompradorByPCID(int pcid)
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
                            return getComprador(Convert.ToInt32(dataReader["compra_id"]));
                        }
                    }
                    connection.Close();
                }
            }
            return "";
        }

        public int getCompraByPCID(int pcid)
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
                            return Convert.ToInt32(dataReader["compra_id"]);
                        }
                    }
                    connection.Close();
                }
            }
            return -1;
        }

        public IActionResult BriefingsPorAceitar()
        {
            List<Briefing> briefings = new List<Briefing>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Briefing";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            Briefing briefing = new Briefing();
                            briefing.id = Convert.ToInt32(dataReader["id"]);
                            briefing.empresa = Convert.ToString(dataReader["empresa"]);
                            briefing.setor = Convert.ToString(dataReader["setor"]);
                            briefing.historia_empresa = Convert.ToString(dataReader["historia_empresa"]);
                            briefing.objetivo_negocio = Convert.ToString(dataReader["objetivo_negocio"]);
                            briefing.estrategia = Convert.ToString(dataReader["estrategia"]);
                            briefing.produtos_comercializados = Convert.ToString(dataReader["produtos_comercializados"]);
                            briefing.marca = Convert.ToString(dataReader["marca"]);
                            briefing.imagem_corporativa = Convert.ToString(dataReader["imagem_corporativa"]);
                            briefing.posicionamento = Convert.ToString(dataReader["posicionamento"]);
                            briefing.publico_alvo = Convert.ToString(dataReader["publico_alvo"]);
                            briefing.concorrentes = Convert.ToString(dataReader["concorrentes"]);
                            briefing.objetivos = Convert.ToString(dataReader["objetivos"]);
                            briefing.resultados_esperados = Convert.ToString(dataReader["resultados_esperados"]);
                            briefing.permissas = Convert.ToString(dataReader["permissas"]);
                            briefing.restricoes = Convert.ToString(dataReader["restricoes"]);
                            briefing.data_entrega = Convert.ToDateTime(dataReader["data_entrega"]).Date;
                            briefing.cronograma_1 = Convert.ToDateTime(dataReader["cronograma_1"]).Date;
                            briefing.cronograma_2 = Convert.ToDateTime(dataReader["cronograma_2"]).Date;
                            briefing.cronograma_3 = Convert.ToDateTime(dataReader["cronograma_3"]).Date;
                            briefing.linha_seguir = Convert.ToString(dataReader["linha_seguir"]);
                            briefing.tom_voz = Convert.ToString(dataReader["tom_voz"]);
                            briefing.tipo_letra = Convert.ToString(dataReader["tipo_letra"]);
                            briefing.cor = Convert.ToString(dataReader["cor"]);
                            briefing.aceite = Convert.ToBoolean(dataReader["aceite"]);
                            briefing.produtoCompraID = Convert.ToInt32(dataReader["produto_compra_id"]);
                            if (!briefing.aceite)
                            {
                                briefings.Add(briefing);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            List<BriefingPorAceitar> briefingPorAceitar = new List<BriefingPorAceitar>();
            foreach (Briefing b in briefings)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"SELECT * FROM Produto_Compra WHERE id = {b.produtoCompraID}";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                BriefingPorAceitar bpa = new BriefingPorAceitar();
                                bpa.id = b.id;
                                bpa.produto = getProductByID(Convert.ToInt32(dataReader["produto_id"]));
                                bpa.comprador = getComprador(Convert.ToInt32(dataReader["compra_id"]));
                                if (getEstado(Convert.ToInt32(dataReader["compra_id"])).Equals("Planeamento"))
                                {
                                    briefingPorAceitar.Add(bpa);
                                }
                            }
                        }
                        connection.Close();
                    }
                }
            }        
            return View(briefingPorAceitar);
        }

        public String getEstado(int idCompra)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Compra WHERE id = {idCompra}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (Convert.ToInt32(dataReader["id"]) == idCompra)
                            {
                                return getNomeEstado(Convert.ToInt32(dataReader["estado_id"]));
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return "";
        }

        public String getComprador(int idCompra)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Compra WHERE id = {idCompra}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (Convert.ToInt32(dataReader["id"]) == idCompra)
                            {
                                return getNomeComprador(Convert.ToInt32(dataReader["cliente_id"]));
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return "";
        }

        public String getNomeComprador(int idCliente)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Cliente WHERE id = {idCliente}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (Convert.ToInt32(dataReader["id"]) == idCliente)
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

        
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult VerBriefing(int idBriefing)
        {   
           
                Briefing briefing = new Briefing();
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"SELECT * FROM Briefing Where id={Convert.ToInt32(idBriefing)}";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                briefing.id = Convert.ToInt32(dataReader["id"]);
                                briefing.empresa = Convert.ToString(dataReader["empresa"]);
                                briefing.setor = Convert.ToString(dataReader["setor"]);
                                briefing.historia_empresa = Convert.ToString(dataReader["historia_empresa"]);
                                briefing.objetivo_negocio = Convert.ToString(dataReader["objetivo_negocio"]);
                                briefing.estrategia = Convert.ToString(dataReader["estrategia"]);
                                briefing.produtos_comercializados = Convert.ToString(dataReader["produtos_comercializados"]);
                                briefing.marca = Convert.ToString(dataReader["marca"]);
                                briefing.imagem_corporativa = Convert.ToString(dataReader["imagem_corporativa"]);
                                briefing.posicionamento = Convert.ToString(dataReader["posicionamento"]);
                                briefing.publico_alvo = Convert.ToString(dataReader["publico_alvo"]);
                                briefing.concorrentes = Convert.ToString(dataReader["concorrentes"]);
                                briefing.objetivos = Convert.ToString(dataReader["objetivos"]);
                                briefing.resultados_esperados = Convert.ToString(dataReader["resultados_esperados"]);
                                briefing.permissas = Convert.ToString(dataReader["permissas"]);
                                briefing.restricoes = Convert.ToString(dataReader["restricoes"]);
                                briefing.data_entrega = Convert.ToDateTime(dataReader["data_entrega"]).Date;
                                briefing.cronograma_1 = Convert.ToDateTime(dataReader["cronograma_1"]).Date;
                                briefing.cronograma_2 = Convert.ToDateTime(dataReader["cronograma_2"]).Date;
                                briefing.cronograma_3 = Convert.ToDateTime(dataReader["cronograma_3"]).Date;
                                briefing.linha_seguir = Convert.ToString(dataReader["linha_seguir"]);
                                briefing.tom_voz = Convert.ToString(dataReader["tom_voz"]);
                                briefing.tipo_letra = Convert.ToString(dataReader["tipo_letra"]);
                                briefing.cor = Convert.ToString(dataReader["cor"]);
                            }
                        }
                        connection.Close();
                    }
                }
                return View(briefing);
        }

        public IActionResult AceitarBriefing(int idBriefing)
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update briefing SET aceite='true'  WHERE id={idBriefing} ";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }

                sql = $"Insert Into Projeto (versao, versao_final, id_briefing) Values (0,'false',{idBriefing})";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return RedirectToAction("Index", "AreaCM");

        }
    }
}
