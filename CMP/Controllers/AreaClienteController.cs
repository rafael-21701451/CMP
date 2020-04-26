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
            return View(compra);
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
