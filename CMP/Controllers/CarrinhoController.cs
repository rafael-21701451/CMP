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

        public IActionResult Index()
        {
            Carrinho carrinho = new Carrinho();
            int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            if (temCompraPorConfirmar(idCliente))
            {
                Compra compra = getCompraByCliente(idCliente);
                carrinho.subtotal = compra.subTotal;
                carrinho.total = compra.total;
                List<int> idProdutosCompra = getIDProdutosCompra(compra.id);
                List<Product> produtosCarrinho = new List<Product>();
                foreach(int i in idProdutosCompra)
                {
                    produtosCarrinho.Add(getProductByID(i));
                }
                carrinho.produtos = produtosCarrinho;
                return View(carrinho);
            }
            else
            {
                return View(carrinho);
            }
            
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
                string sql = $"SELECT top 1 * FROM Compra WHERE cliente_id={id} AND estado_id={1} ORDER BY id DESC";
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
                            if (Convert.ToInt32(dataReader["cliente_id"]) == id && Convert.ToInt32(dataReader["estado_id"]) == 1)
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

        public List<int> getIDProdutosCompra(int idCompra)
        {
            List<int> ids = new List<int>();
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
                                ids.Add(Convert.ToInt32(dataReader["produto_id"]));
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return ids;
        }

        public Product getProductByID(int idProduto)
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
                            if (Convert.ToInt32(dataReader["id"]) == idProduto)
                            {
                                product.id = Convert.ToInt32(dataReader["id"]);
                                product.nome = Convert.ToString(dataReader["nome"]);
                                product.preco = Convert.ToDouble(dataReader["preco"]);
                                product.categoria = getProductCategory(product.id);
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
    }
}
