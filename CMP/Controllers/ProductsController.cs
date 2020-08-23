using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CMP.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace CMP.Controllers
{
    public class ProductsController : Controller
    {

        private readonly IConfiguration _configuration;

        public ProductsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ArtigosBlog()
        {
            return View();
        }

        public IActionResult ArtigoBlogA()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ArtigoBlogA(Briefing briefing)
        {
            Product product = getProductByName("Criação de 4 textos de 500 palavras");
            string sql = "";
            int idCliente = getidCliente(Convert.ToInt32(this.User.Claims.ElementAt(2).Value));
            Compra compra = new Compra();
            if (ModelState.IsValid)
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    if (temCompraPorConfirmar(idCliente))
                    {
                        compra = getCompraByCliente(idCliente);

                        sql = $"Update Compra SET sub_total='{Convert.ToString(compra.subTotal + product.preco).Replace(',', '.')}', total='{Convert.ToString(compra.subTotal + product.preco + ((compra.subTotal + product.preco) * 0.23)).Replace(',', '.')}' Where id='{compra.id}'";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                    }
                    else
                    {
                        sql = $"Insert Into Compra (sub_total, iva, total, estado_id, cliente_id) Values ('{Convert.ToString(product.preco).Replace(',', '.')}','{23}','{Convert.ToString(product.preco + ((product.preco) * 0.23)).Replace(',', '.')}','{getEstadoByNome("Por Pagar")}','{idCliente}')";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.CommandType = System.Data.CommandType.Text;
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                        }

                        compra = getCompraByCliente(idCliente);
                    }


                    sql = $"Insert Into Produto_Compra (valor_desconto, compra_id, produto_id, quantidade) Values ('{0}','{compra.id}','{product.id}','{1}')";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = System.Data.CommandType.Text;
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }

                    int pc_id = -1;

                    sql = $"SELECT top 1 * FROM Produto_Compra WHERE compra_id={compra.id} ORDER BY id DESC";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                pc_id = Convert.ToInt32(dataReader["id"]);
                            }
                        }
                        connection.Close();
                    }

                    sql = $"Insert Into Briefing (empresa, setor, historia_empresa,objetivo_negocio, estrategia, produtos_comercializados, marca, imagem_corporativa" +
                        $",posicionamento,publico_alvo,concorrentes,objetivos,resultados_esperados,permissas,restricoes,data_entrega,cronograma_1,cronograma_2,cronograma_3" +
                        $",linha_seguir,tom_voz,tipo_letra,cor,aceite,produto_compra_id,revisao) " +
                        $"Values (@empresa, @setor, @historia_empresa, @objetivo_negocio, @estrategia, @produtos_comercializados, @marca, @imagem_corporativa" +
                        $", @posicionamento, @publico_alvo, @concorrentes, @objetivos, @resultados_esperados, @permissas, @restricoes, @data_entrega, @cronograma_1, @cronograma_2, @cronograma_3" +
                        $", @linha_seguir, @tom_voz, @tipo_letra, @cor,'false','{pc_id}','false')";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.AddWithValue("@empresa", briefing.empresa);
                        command.Parameters.AddWithValue("@setor", briefing.setor);
                        command.Parameters.AddWithValue("@historia_empresa", briefing.historia_empresa);
                        command.Parameters.AddWithValue("@objetivo_negocio", briefing.objetivo_negocio);
                        command.Parameters.AddWithValue("@estrategia", briefing.estrategia);
                        command.Parameters.AddWithValue("@produtos_comercializados", briefing.produtos_comercializados);
                        command.Parameters.AddWithValue("@marca", briefing.marca);
                        command.Parameters.AddWithValue("@imagem_corporativa", briefing.imagem_corporativa);
                        command.Parameters.AddWithValue("@posicionamento", briefing.posicionamento);
                        command.Parameters.AddWithValue("@publico_alvo", briefing.publico_alvo);
                        command.Parameters.AddWithValue("@concorrentes", briefing.concorrentes);
                        command.Parameters.AddWithValue("@objetivos", briefing.objetivos);
                        command.Parameters.AddWithValue("@resultados_esperados", briefing.resultados_esperados);
                        command.Parameters.AddWithValue("@permissas", briefing.permissas);
                        command.Parameters.AddWithValue("@restricoes", briefing.restricoes);
                        command.Parameters.AddWithValue("@data_entrega", briefing.data_entrega);
                        command.Parameters.AddWithValue("@cronograma_1", briefing.cronograma_1);
                        command.Parameters.AddWithValue("@cronograma_2", briefing.cronograma_2);
                        command.Parameters.AddWithValue("@cronograma_3", briefing.cronograma_3);
                        command.Parameters.AddWithValue("@linha_seguir", briefing.linha_seguir);
                        command.Parameters.AddWithValue("@tom_voz", briefing.tom_voz);
                        command.Parameters.AddWithValue("@tipo_letra", briefing.tipo_letra);
                        command.Parameters.AddWithValue("@cor", briefing.cor);
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }


                }
                return RedirectToAction("Index", "Carrinho");
            }
            else
            {
                return View();
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

        public Product getProductByName(String name)
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
                            if (Convert.ToString(dataReader["nome"]).Equals(name))
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
