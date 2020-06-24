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
            DadosProdutor dp = new DadosProdutor();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Produtor WHERE id={Convert.ToInt32(this.User.Claims.ElementAt(2).Value)}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            dp.temMensagens = temMensagens(Convert.ToInt32(dataReader["account_id"]));
                        }
                    }
                    connection.Close();
                }
            }
            dp.projetosAtuais = getProjetosAtuais(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            dp.projetosFinalizados = getProjetosFinalizados(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            dp.projetosEmAprovacao = getProjetosPorAprovar(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            return View(dp);
        }

        public IActionResult Mensagens()
        {
            List<MensagemProdutor> mensagens = new List<MensagemProdutor>();
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
              

                int idConta = -1;
                string sql = $"SELECT * FROM Produtor WHERE id={Convert.ToInt32(this.User.Claims.ElementAt(2).Value)}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            idConta = Convert.ToInt32(dataReader["account_id"]);
                        }
                    }
                    connection.Close();
                }

               

                sql = $"Update Account SET new_messages='false' WHERE id={idConta}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }

                 sql = $"SELECT * FROM Mensagem WHERE Destinatario={idConta}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            MensagemProdutor msg = new MensagemProdutor();
                            msg.id = Convert.ToInt32(dataReader["id"]);
                            msg.assunto = Convert.ToString(dataReader["Assunto"]);
                            msg.remetente = getNomeCMbyAccountID(Convert.ToInt32(dataReader["Remetente"]));
                            mensagens.Add(msg);
                        }
                    }
                    connection.Close();
                }

            }
            return View(mensagens);
        }

        public String getNomeCMbyAccountID(int idAccount) {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Content_Manager WHERE account_id={idAccount}";
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


        public Boolean temMensagens(int accID)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Account WHERE id={accID}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            return Convert.ToBoolean(dataReader["new_messages"]);
                        }
                    }
                    connection.Close();
                }
            }
            return false;
        }

        public int getProjetosAtuais(int idProdutor)
        {
            int count = 0;
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
                            int id = getIdProjeto(Convert.ToInt32(dataReader["projeto_id"]));
                            int briefingID = getIdBriefing(id);
                            int pcID = getPCByBriefingID(briefingID);
                            if (!getEstadoProjeto(id))
                            {
                                count++;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return count;
        }

        public int getProjetosPorAprovar(int idProdutor)
        {
            int count = 0;
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
                            int id = getIdProjeto(Convert.ToInt32(dataReader["projeto_id"]));
                            int briefingID = getIdBriefing(id);
                            int pcID = getPCByBriefingID(briefingID);
                            int idCompra = getCompraByPCID(pcID);
                            if (getEstadoIDByNome("Aceitação") == getEstadoIDCompra(idCompra) || getEstadoIDByNome("Concluído") == getEstadoIDCompra(idCompra) || getEstadoIDByNome("Validação") == getEstadoIDCompra(idCompra))
                            {
                                count++;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return count;
        }

        public int getProjetosFinalizados(int idProdutor)
        {
            int count = 0;
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
                            int id = getIdProjeto(Convert.ToInt32(dataReader["projeto_id"]));
                            int briefingID = getIdBriefing(id);
                            int pcID = getPCByBriefingID(briefingID);
                            int idCompra = getCompraByPCID(pcID);
                            if (getEstadoIDByNome("Entregue") == getEstadoIDCompra(idCompra))
                            {
                                count++;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return count;
        }



        public IActionResult ProjetosEmAprovacao()
        {
            List<ProjetoAprovacao> projetosAprovacao = new List<ProjetoAprovacao>();
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
                            ProjetoAprovacao pa = new ProjetoAprovacao();
                            pa.id = getIdProjeto(Convert.ToInt32(dataReader["projeto_id"]));
                            int briefingID = getIdBriefing(pa.id);
                            int pcID = getPCByBriefingID(briefingID);
                            pa.produto = getProductByPCID(pcID);
                            pa.atribuidor = getNomeCMByID(Convert.ToInt32(dataReader["content_manager_id"]));
                            int idCompra = getCompraByPCID(pcID);
                            if(getEstadoIDByNome("Aceitação") == getEstadoIDCompra(idCompra) || getEstadoIDByNome("Concluído") == getEstadoIDCompra(idCompra) || getEstadoIDByNome("Validação") == getEstadoIDCompra(idCompra))
                            {
                                projetosAprovacao.Add(pa);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return View(projetosAprovacao);
        }

        public IActionResult ProjetosFinalizados()
        {
            List<ProjetoFinalizado> projetosAprovacao = new List<ProjetoFinalizado>();
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
                            ProjetoFinalizado pf = new ProjetoFinalizado();
                            pf.id = getIdProjeto(Convert.ToInt32(dataReader["projeto_id"]));
                            int briefingID = getIdBriefing(pf.id);
                            int pcID = getPCByBriefingID(briefingID);
                            pf.produto = getProductByPCID(pcID);
                            pf.atribuidor = getNomeCMByID(Convert.ToInt32(dataReader["content_manager_id"]));
                            int idCompra = getCompraByPCID(pcID);
                            if (getEstadoIDByNome("Entregue") == getEstadoIDCompra(idCompra))
                            {
                                projetosAprovacao.Add(pf);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return View(projetosAprovacao);
        }


        public IActionResult UploadProjeto(int idProjeto)
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                int versaoProjeto = VerVersaoProjeto(idProjeto);
                string sql = $"Update Projeto SET versao={versaoProjeto}+1, versao_final='true' WHERE id={idProjeto} ";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }

                int idCompra = -1;
                sql = $"SELECT * FROM Projeto WHERE id={idProjeto}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            int pcID = getPCByBriefingID(Convert.ToInt32(dataReader["id_briefing"]));
                            idCompra = getCompraByPCID(pcID);
                        }
                    }
                    connection.Close();
                }

                int idEstado = -1;
                idEstado = getEstadoByNome("Aceitação");

                sql = $"Update Compra SET estado_id={idEstado} WHERE id={idCompra}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }
            }
            return RedirectToAction("Index", "AreaProdutor");
        }

        public int getEstadoIDByNome(String estado)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Estado WHERE estado = '{estado}'";
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

        public int getEstadoIDCompra(int idCompra)
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
                            return Convert.ToInt32(dataReader["estado_id"]);
                        }
                    }
                    connection.Close();
                }
            }
            return -1;
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

        public int getEstadoByNome(string nomeEstado)
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
                            if (Convert.ToString(dataReader["estado"]).Equals(nomeEstado))
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

        public IActionResult VerProjetoAtual(int idProjeto)
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
                            ppa.briefingID = Convert.ToInt32(dataReader["id_briefing"]);
                        }
                    }
                    connection.Close();
                }
            }
            return View(ppa);
        }

        public IActionResult VerProjetoPorAprovar(int idProjeto)
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
                            ppa.briefingID = Convert.ToInt32(dataReader["id_briefing"]);
                        }
                    }
                    connection.Close();
                }
            }
            return View(ppa);
        }

        public IActionResult VerProjetoFinalizado(int idProjeto)
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
                            ppa.briefingID = Convert.ToInt32(dataReader["id_briefing"]);
                        }
                    }
                    connection.Close();
                }
            }
            return View(ppa);
        }

        public int VerVersaoProjeto(int idProjeto)
        {
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
                            return Convert.ToInt32(dataReader["versao"]);
                        }
                    }
                    connection.Close();
                }
            }
            return -1;
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

        public IActionResult VerBriefingAceite(int idBriefing)
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
    }
}
