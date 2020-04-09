using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class Compra
    {
        public int id { get; set; }
        public double subTotal { get; set; }
        public int iva { get; set; }
        public double total { get; set; }
    }
}
