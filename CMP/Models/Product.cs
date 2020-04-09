using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class Product
    {
        public int id { get; set; }
        public string nome { get; set; }
        public double preco { get; set; }
        public string categoria { get; set; }

        public int categoriaId { get; set; }
    }
}
