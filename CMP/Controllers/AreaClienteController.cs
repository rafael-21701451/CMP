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

namespace CMP.Controllers
{
    [Authorize]
    public class AreaClienteController : Controller
    {

        private readonly IConfiguration _configuration;

        public AreaClienteController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Mensagens()
        {
            List<MensagemProdutor> mensagens = new List<MensagemProdutor>();
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {


                int idConta = -1;
                string sql = $"SELECT * FROM Cliente WHERE id={Convert.ToInt32(this.User.Claims.ElementAt(2).Value)}";
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
                            mensagens.Add(msg);
                        }
                    }
                    connection.Close();
                }

            }
            return View(mensagens);
        }

        public IActionResult EditarDados(int id)
        {
            if (id != Convert.ToInt32(User.Claims.ElementAt(2).Value))
            {
                return RedirectToAction("DadosPessoais");
            }
            else
            {
                EditarPerfilView dados = new EditarPerfilView();
                dados.id = id;
                DadosPerfil d = getAccountDetails(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
                dados.username = d.username;
                dados.email = d.email;
                int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
                dados.nome = getClientName(idCliente);
                return View(dados);
            }

        }

        [HttpPost]
        public async Task<IActionResult> EditarDados(EditarPerfilView dados)
        {
            if (ModelState.IsValid)
            {
                bool valid = true;
                DadosPerfil d = getAccountDetails(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
                if (!verificarUsername(dados.username) && !d.username.Equals(dados.username))
                {
                    valid = false;
                    ModelState.AddModelError("username", "Username já existe");
                }
                if (!verificarEmail(dados.email) && !d.email.Equals(dados.email))
                {
                    valid = false;
                    ModelState.AddModelError("email", "Email já existe");
                }
                if (!dados.password.Equals(dados.passwordCmf))
                {
                    valid = false;
                    ModelState.AddModelError("passwordCmf", "Passwords Não Coincidem");
                }
                if (valid == false)
                {
                    return View(dados);
                }
                int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
                AtualizarPerfil(dados, Convert.ToInt32(this.User.Claims.ElementAt(2).Value), idCliente);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                var claims = new[] {
                                new Claim(ClaimTypes.Name, dados.email),//0
                                new Claim("nome", dados.nome),//1
                                new Claim("id", Convert.ToString(getIdByEmail(dados.email))),//2
                            };
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProperties);
                return RedirectToAction("DadosPessoais");
            }
            else
            {
                return View(dados);
            }
        }

        public IActionResult ConfirmarRececao(int idProdutoCompra)
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
               
                int idCompra = getCompraByPCID(idProdutoCompra);
               
                int idEstado = -1;
                idEstado = getEstadoByNome("Validação");

                string sql = $"Update Compra SET estado_id={idEstado} WHERE id={idCompra}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }
            }
            return RedirectToAction("VerCompra");
        }

        public IActionResult ValidarProduto(int idProdutoCompra)
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                int idCompra = getCompraByPCID(idProdutoCompra);

                int idEstado = -1;
                idEstado = getEstadoByNome("Entregue");

                string sql = $"Update Compra SET estado_id={idEstado} WHERE id={idCompra}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }
            }
            return RedirectToAction("VerCompra");
        }
        public IActionResult RazaoRejeicao(int idProdutoCompra)
        {
            RejectionView rv = new RejectionView();
            rv.idProjeto = idProdutoCompra;
            return View(rv);
        }

        [HttpPost]
        public IActionResult RazaoRejeicao(RejectionView rv)
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                int idCompra = getCompraByPCID(rv.idProjeto);

                int idEstado = -1;
                idEstado = getEstadoByNome("Produção");

                String sql = $"Update Compra SET estado_id={idEstado} WHERE id={idCompra}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }

                int idBriefing = -1;
                sql = $"SELECT * FROM Briefing WHERE produto_compra_id={rv.idProjeto}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            idBriefing = Convert.ToInt32(dataReader["id"]);
                        }
                    }
                    connection.Close();
                }

                int idProjeto = -1;
                sql = $"SELECT * FROM Projeto WHERE id_briefing={idBriefing}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            idProjeto = Convert.ToInt32(dataReader["id"]);
                        }
                    }
                    connection.Close();
                }




                sql = $"Update Projeto SET versao_final='false' WHERE id={idProjeto}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }


                int idDestinatario1 = -1;
                int idDestinatario2 = -1;
                sql = $"SELECT * FROM Produtor_Projeto WHERE projeto_id={idProjeto}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            idDestinatario1 = getAccountIDProdutorByID(Convert.ToInt32(dataReader["produtor_id"]));
                            idDestinatario2 = getAccountIDCMByID(Convert.ToInt32(dataReader["content_manager_id"]));
                        }
                    }
                    connection.Close();
                }

                sql = $"Insert Into Mensagem (Mensagem, Remetente, Destinatario, Assunto) Values ('{rv.mensagem}',{Convert.ToInt32(this.User.Claims.ElementAt(2).Value)},{idDestinatario2},'Projeto #{idProjeto} rejeitado')";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }

                sql = $"Insert Into Mensagem (Mensagem, Remetente, Destinatario, Assunto) Values ('{rv.mensagem}',{idDestinatario2},{idDestinatario1},'Projeto #{idProjeto} rejeitado')";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }

                sql = $"Update Account SET new_messages='true' WHERE id={idDestinatario2}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }

                sql = $"Update Account SET new_messages='true' WHERE id={idDestinatario1}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }


            }
            return RedirectToAction("Index");
        }

        public int getAccountIDProdutorByID(int idProdutor)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Produtor WHERE id={idProdutor}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            return Convert.ToInt32(dataReader["account_id"]);
                        }
                    }
                    connection.Close();
                }
            }
            return -1;
        }

        public int getAccountIDCMByID(int idProdutor)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Content_Manager WHERE id={idProdutor}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            return Convert.ToInt32(dataReader["account_id"]);
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

        public IActionResult VerCompra(int id)
        {
            int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            if (compraPertenceCliente(id, idCliente))
            {
                CompraView encomenda = new CompraView();
                encomenda.compra = new Compra();
                encomenda.compra.id = id;
                encomenda.compra.total = getPrecoCompraByID(id);
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"SELECT * FROM Compra WHERE id={id}";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                encomenda.estado = getNomeEstado(Convert.ToInt32(dataReader["estado_id"]));
                            }
                        }
                        connection.Close();
                    }
                }
                encomenda.fatura = new FinalizarCompraView();
                encomenda.fatura = getFatura(id);
                List<Product> ProdutosCompra = getProdutosCompra(id);
                List<Product> produtos = new List<Product>();
                foreach (Product p in ProdutosCompra)
                {
                    produtos.Add(getProductByID(p));
                }
                encomenda.produtos = produtos;
                return View(encomenda);
            }
            else
            {
                return RedirectToAction("MinhasEncomendas");
            }

        }

        public IActionResult Index()
        {
            int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            UltimaCompra compra = getUltimaCompraByCliente(idCliente);
            List<Product> ProdutosCompra = getProdutosCompra(compra.id);
            List<Product> produtos = new List<Product>();
            foreach (Product p in ProdutosCompra)
            {
                produtos.Add(getProductByID(p));
            }
            compra.produtos = produtos;
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
                            compra.temMensagens = temMensagens(Convert.ToInt32(dataReader["account_id"]));
                        }
                    }
                    connection.Close();
                }
            }
            return View(compra);
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


        public IActionResult DadosPessoais()
        {
            DadosPerfil dados = new DadosPerfil();
            DadosPerfil d = getAccountDetails(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            dados.username = d.username;
            dados.email = d.email;
            int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            List<int> compras = new List<int>();
            dados.nome = getClientName(idCliente);
            dados.comprasEfetuadas = 0;
            dados.produtosAdquiridos = 0;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Compra WHERE cliente_id={idCliente} AND estado_id != {getEstadoByNome("Por Pagar")}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            dados.comprasEfetuadas++;
                        }
                    }
                    connection.Close();
                }

                sql = $"SELECT * FROM Compra WHERE cliente_id={idCliente}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            string estado = getNomeEstado(Convert.ToInt32(dataReader["estado_id"]));
                            if (!estado.Equals("Por Pagar"))
                            {
                                compras.Add(Convert.ToInt32(dataReader["id"]));
                            }
                        }
                    }
                    connection.Close();
                }

                sql = $"SELECT * FROM Produto_Compra ";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            foreach (var c in compras)
                            {
                                if (c == Convert.ToInt32(dataReader["compra_id"]))
                                {
                                    dados.produtosAdquiridos++;
                                }
                            }
                        }
                    }
                    connection.Close();
                }

            }
            return View(dados);
        }

        public IActionResult MinhasEncomendas()
        {
            int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            List<CompraPerfil> compras = new List<CompraPerfil>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Compra WHERE cliente_id={idCliente}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            CompraPerfil compra = new CompraPerfil();
                            compra.id = Convert.ToInt32(dataReader["id"]);
                            compra.data = getDataCompra(Convert.ToInt32(dataReader["id"]));
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

        public Boolean compraPertenceCliente(int idCompra,int idCliente)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Compra WHERE id={idCompra}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (Convert.ToInt32(dataReader["cliente_id"]) == idCliente)
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

        public FinalizarCompraView getFatura(int idCompra)
        {
            FinalizarCompraView fatura = new FinalizarCompraView();
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
                            fatura.nome = Convert.ToString(dataReader["nome"]);
                            fatura.morada = Convert.ToString(dataReader["morada"]);
                            fatura.nif = Convert.ToString(dataReader["nif"]);
                            fatura.tlm = Convert.ToString(dataReader["telemovel"]);
                            fatura.email = Convert.ToString(dataReader["email"]);
                        }
                    }
                    connection.Close();
                }
            }
            return fatura;
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

        public UltimaCompra getUltimaCompraByCliente(int id)
        {
            UltimaCompra compra = new UltimaCompra();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT top 1 * FROM Compra WHERE cliente_id={id} AND estado_id != {getEstadoByNome("Por Pagar")} ORDER BY id DESC";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (Convert.ToInt32(dataReader["estado_id"]) != getEstadoByNome("Por Pagar"))
                            {
                                compra.id = Convert.ToInt32(dataReader["id"]);
                                compra.estado = getEstadoById(Convert.ToInt32(dataReader["estado_id"]));
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return compra;
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

        public string getEstadoById(int idEstado)
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

        public int getidCliente(int id)
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
                            if (Convert.ToInt32(dataReader["account_id"]) == id)
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

        public string getClientName(int id)
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
                            if (Convert.ToInt32(dataReader["id"]) == id)
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

        public List<Product> getProdutosCompra(int idCompra)
        {
            List<Product> ProdutoCompra = new List<Product>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Produto_Compra";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (Convert.ToInt32(dataReader["compra_id"]) == idCompra)
                            {
                                Product produto = new Product();
                                produto.idProdutoCompra = Convert.ToInt32(dataReader["id"]);
                                produto.id = Convert.ToInt32(dataReader["produto_id"]);
                                produto.quantidade = Convert.ToInt32(dataReader["quantidade"]);
                                ProdutoCompra.Add(produto);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return ProdutoCompra;
        }

        public Product getProductByID(Product p)
        {
            Product product = new Product();
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
                            if (Convert.ToInt32(dataReader["id"]) == p.id)
                            {
                                product.id = Convert.ToInt32(dataReader["id"]);
                                product.nome = Convert.ToString(dataReader["nome"]);
                                product.preco = Convert.ToDouble(dataReader["preco"]);
                                product.categoria = getProductCategory(Convert.ToInt32(dataReader["categoria_id"]));
                                product.idProdutoCompra = p.idProdutoCompra;
                                product.quantidade = p.quantidade;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return product;
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

        public int getPrecoCompraByID(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Compra WHERE id={id}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                                return Convert.ToInt32(dataReader["total"]);
                        }
                    }
                    connection.Close();
                }
            }
            return -1;
        }

        public DadosPerfil getAccountDetails(int id)
        {
            DadosPerfil dados = new DadosPerfil();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM Account WHERE id = {id}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            dados.username = Convert.ToString(dataReader["username"]);
                            dados.email = Convert.ToString(dataReader["email"]);
                        }
                    }
                    connection.Close();
                }
            }
            return dados;
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

        public void AtualizarPerfil(EditarPerfilView dados, int idAccount, int idClient)
        {
            String password = dados.password;
            HashAlgorithm hasher;
            hasher = new SHA256Managed();
            byte[] mPasswordBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(password);
            byte[] mPasswordHash = hasher.ComputeHash(mPasswordBytes);
            password = Convert.ToBase64String(mPasswordHash, 0, mPasswordHash.Length);

            string connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update Account SET username='{dados.username}', email='{dados.email}', password='{password}' Where id='{idAccount}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    
                }

                sql = $"Update Cliente SET nome='{dados.nome}' Where id='{idClient}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }


            }
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

    }
}
