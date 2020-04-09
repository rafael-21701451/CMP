using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class Carrinho
    {
        public IEnumerable<Product> produtos { get; set; }

        public double subtotal { get; set; }

        public double total { get; set; }

    }
}
