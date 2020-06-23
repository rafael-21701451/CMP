using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class Produtor
    {
        public int id { get; set; }
        public string produtor { get; set; }
        public string especialidade { get; set; }
        public int projetosAtuais { get; set; }

        public int projetoSelecionado { get; set; }
    }
}
