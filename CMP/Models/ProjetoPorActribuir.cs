using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class ProjetoPorAtribuir
    {
        public int id { get; set; }
        public string produto { get; set; }
        public string comprador { get; set; }
        public int idCompra { get; set; }
    }
}
