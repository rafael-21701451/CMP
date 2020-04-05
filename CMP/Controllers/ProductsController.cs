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
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Insert Into Compra (sub_total, iva, total, estado_id, cliente_id) Values ('{200}','{23}','{200 + (200*0.23)}','{1}','{1}')";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                sql = $"Insert Into Produto_Compra (valor_desconto, compra_id, produto_id) Values ('{0}','{1}','{1}')";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                sql = $"Insert Into Briefing (empresa, setor, historia_empresa,objetivo_negocio, estrategia, produtos_comercializados, marca, imagem_corporativa" +
                    $",posicionamento,publico_alvo,concorrentes,objetivos,resultados_esperados,permissas,restricoes,data_entrega,cronograma_1,cronograma_2,cronograma_3" +
                    $",linha_seguir,tom_voz,tipo_letra,cor,aceite,produto_compra_id) " +
                    $"Values ('{briefing.empresa}','{briefing.setor}','{briefing.historia_empresa}','{briefing.objetivo_negocio}','{briefing.estrategia}','{briefing.produtos_comercializados}','{briefing.marca}','{briefing.imagem_corporativa}'" +
                    $",'{briefing.posicionamento}','{briefing.publico_alvo}','{briefing.concorrentes}','{briefing.objetivos}','{briefing.resultados_esperados}','{briefing.permissas}','{briefing.restricoes}','{string.Format("{0:yyyy-MM-dd}", briefing.data_entrega)}','{string.Format("{0:yyyy-MM-dd}", briefing.cronograma_1)}','{string.Format("{0:yyyy-MM-dd}", briefing.cronograma_2)}','{string.Format("{0:yyyy-MM-dd}", briefing.cronograma_3)}'" +
                    $",'{briefing.linha_seguir}','{briefing.tom_voz}','{briefing.tipo_letra}','{briefing.cor}','false','{1}')";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }


            }
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
