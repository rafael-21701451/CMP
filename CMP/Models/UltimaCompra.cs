using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class UltimaCompra
    {
        public IEnumerable<Product> produtos { get; set; }

        public int id { get; set; }

        public string estado { get; set; }

        public Boolean temMensagens { get; set; }

    }
}
