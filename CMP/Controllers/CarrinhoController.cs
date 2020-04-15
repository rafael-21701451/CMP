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
                    string sql = $"Insert Into Fatura (nome, morada, email, telemovel,nif,compra_id) Values ('{finalizar.nome}','{finalizar.morada}','{finalizar.email}','{finalizar.tlm}','{finalizar.nif}','{compra.id}')";
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
                sql = $"Update Compra SET sub_total='{Convert.ToString(compra.subTotal - productPrices.preco).Replace(',', '.')}', total='{Convert.ToString(compra.subTotal - productPrices.preco + ((compra.subTotal - productPrices.preco) * 0.23)).Replace(',', '.')}' Where id='{compra.id}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
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
