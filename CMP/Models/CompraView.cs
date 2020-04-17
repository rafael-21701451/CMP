using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class CompraView
    {
        public Compra compra { get; set; }

        public IEnumerable<Product> produtos { get; set; }

        public FinalizarCompraView fatura { get; set; }

        public string estado { get; set; }

    }
}
