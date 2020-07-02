using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CMP.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace CMP.Controllers
{
    [Authorize]
    public class CarrinhoController : Controller
    {

        private readonly IConfiguration _configuration;

        public CarrinhoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult VerBriefing(int idProdutoCompra)
        {
            int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            if (pcPertenceCliente(idProdutoCompra, idCliente))
            {
                Briefing briefing = new Briefing();
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"SELECT * FROM Briefing Where produto_compra_id={idProdutoCompra}";
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
            else
            {
                return RedirectToAction("MinhasEncomendas", "AreaCliente");
            }
            
        }   

        public IActionResult Pagamento()
        {
            FinalizarCompraView compraFin = new FinalizarCompraView();
            int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            if (temCompraPorConfirmar(idCliente))
            {
                Compra compra = getCompraByCliente(idCliente);
                compraFin.subtotal = compra.subTotal;
                compraFin.total = compra.total;
                return View(compraFin);
            }
            else
            {
                return View(compraFin);
            }
        }

        public IActionResult Adicionar(int idProdutoCompra)
        {
            Product pc = getProductIDByProduto_Compra(idProdutoCompra);
            Product pc2 = getProductByID(pc);
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update Produto_Compra SET quantidade='{pc2.quantidade + 1}' Where id='{idProdutoCompra}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }

                int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
                Compra compra = getCompraByCliente(idCliente);

                sql = $"Update Compra SET sub_total='{Convert.ToString(compra.subTotal + pc2.preco).Replace(',', '.')}', total='{Convert.ToString(compra.subTotal + pc2.preco + ((compra.subTotal + pc2.preco) * 0.23)).Replace(',', '.')}' Where id='{compra.id}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }


            }
            return RedirectToAction("Index");
        }

        public IActionResult Decrementar(int idProdutoCompra)
        {
            Product pc = getProductIDByProduto_Compra(idProdutoCompra);
            Product pc2 = getProductByID(pc);
            if(pc2.quantidade-1 == 0)
            {
                Remover(idProdutoCompra);
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update Produto_Compra SET quantidade='{pc2.quantidade - 1}' Where id='{idProdutoCompra}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }

                int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
                Compra compra = getCompraByCliente(idCliente);

                sql = $"Update Compra SET sub_total='{Convert.ToString(compra.subTotal - pc2.preco).Replace(',', '.')}', total='{Convert.ToString(compra.subTotal - pc2.preco + ((compra.subTotal - pc2.preco) * 0.23)).Replace(',', '.')}' Where id='{compra.id}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }


            }
            return RedirectToAction("Index");
        }

        public Boolean compraPertenceCliente(int idCompra, int idCliente)
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

        public bool pcPertenceCliente (int idPC, int idCliente)
        {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"SELECT * FROM Produto_Compra WHERE id={idPC}";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                        while (dataReader.Read())
                            {
                            return compraPertenceCliente(Convert.ToInt32(dataReader["compra_id"]), idCliente);
                            }
                        }
                        connection.Close();
                    }
                }
                return false;
        }

        [HttpPost]
        public IActionResult Pagamento(FinalizarCompraView finalizar)
        {
            int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            Compra compra = getCompraByCliente(idCliente);
            if (ModelState.IsValid)
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"Insert Into Fatura (nome, morada, email, telemovel,nif,compra_id, data) Values ('{finalizar.nome}','{finalizar.morada}','{finalizar.email}','{finalizar.tlm}','{finalizar.nif}','{compra.id}','{string.Format("{0:yyyy-MM-dd}", DateTime.Now)}')";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = System.Data.CommandType.Text;
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }

                    sql = $"Update Compra SET estado_id='{getEstadoByNome("Planeamento")}'";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                FinalizarCompraView compraFin = new FinalizarCompraView();
                compraFin.subtotal = compra.subTotal;
                compraFin.total = compra.total;
                return View(compraFin);
            }
        }


        public IActionResult Index()
        {
            Carrinho carrinho = new Carrinho();
            int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            if (temCompraPorConfirmar(idCliente))
            {
                Compra compra = getCompraByCliente(idCliente);
                carrinho.subtotal = compra.subTotal;
                carrinho.total = compra.total;
                List<Product> ProdutosCompra = getProdutosCompra(compra.id);
                List<Product> produtosCarrinho = new List<Product>();
                foreach(Product p in ProdutosCompra)
                {
                    produtosCarrinho.Add(getProductByID(p));
                }
                carrinho.produtos = produtosCarrinho;
                return View(carrinho);
            }
            else
            {
                return View(carrinho);
            }
            
        }

        public IActionResult Remover(int idProdutoCompra)
        {
            Product product = new Product();
            string sql = "";
            product = getProductIDByProduto_Compra(idProdutoCompra);
            Product productPrices = getProductByID(product);
            int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            Compra compra = getCompraByCliente(idCliente);
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                sql = $"DELETE FROM Briefing Where produto_compra_id='{idProdutoCompra}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                sql = $"DELETE FROM Produto_Compra WHERE id='{idProdutoCompra}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                sql = $"Update Compra SET sub_total='{Convert.ToString(compra.subTotal - productPrices.preco).Replace(',', '.')}', total='{Convert.ToString(compra.subTotal - productPrices.preco + ((compra.subTotal - productPrices.preco) * 0.23)).Replace(',', '.')}' Where id='{compra.id}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                if(compra.subTotal - productPrices.preco == 0)
                {
                    sql = $"DELETE FROM Compra WHERE id='{compra.id}'";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }
           
            deleteBriefing(idProdutoCompra);
            deleteProdutoCompra(idProdutoCompra);
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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

        public Compra getCompraByCliente(int id)
        {
            Compra compra = new Compra();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT top 1 * FROM Compra WHERE cliente_id={id} AND estado_id={getEstadoByNome("Por Pagar")} ORDER BY id DESC";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (Convert.ToInt32(dataReader["cliente_id"]) == id)
                            {
                                compra.id = Convert.ToInt32(dataReader["id"]);
                                compra.iva = Convert.ToInt32(dataReader["iva"]);
                                compra.subTotal = Convert.ToDouble(dataReader["sub_total"]);
                                compra.total = Convert.ToDouble(dataReader["total"]);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return compra;
        }

        public bool temCompraPorConfirmar(int id)
        {
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
                            if (Convert.ToInt32(dataReader["cliente_id"]) == id && Convert.ToInt32(dataReader["estado_id"]) == getEstadoByNome("Por Pagar"))
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
                                product.categoria = getProductCategory(product.id);
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

        public Product getProductIDByProduto_Compra(int pc)
        {
            Product product = new Product();
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
                            if (Convert.ToInt32(dataReader["id"]) == pc)
                            {
                                product.id = Convert.ToInt32(dataReader["produto_id"]);
                                product.quantidade = Convert.ToInt32(dataReader["quantidade"]);
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

        public void deleteBriefing(int idProdutoCompra)
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Delete From Briefing Where produto_compra_id='{idProdutoCompra}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public void deleteProdutoCompra(int idProdutoCompra)
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Delete From Produto_Compra Where id='{idProdutoCompra}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
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
    }
}
