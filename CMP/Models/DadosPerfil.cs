using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class DadosPerfil
    {
        public string nome { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public int produtosAdquiridos { get; set; }
        public int comprasEfetuadas { get; set; }
    }
}
