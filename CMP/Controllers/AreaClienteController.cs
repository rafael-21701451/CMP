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
            int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            UltimaCompra compra = getUltimaCompraByCliente(idCliente);
            List<Product> ProdutosCompra = getProdutosCompra(compra.id);
            List<Product> produtos= new List<Product>();
            foreach (Product p in ProdutosCompra)
            {
                produtos.Add(getProductByID(p));
            }
            compra.produtos = produtos;
            return View(compra);
        }

        public IActionResult DadosPessoais()
        {
            DadosPerfil dados = new DadosPerfil();
            DadosPerfil d = getAccountDetails(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            dados.username = d.username;
            dados.email = d.email;
            int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            dados.nome = getClientName(idCliente);
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
                            if (Convert.ToInt32(dataReader["estado_id"])!=getEstadoByNome("Por Pagar"))
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
    }
}
