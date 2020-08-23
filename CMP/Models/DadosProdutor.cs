using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class DadosProdutor
    {
        public int projetosAtuais { get; set; }
        public int projetosEmAprovacao { get; set; }
        public int projetosFinalizados { get; set; }

        public Boolean temMensagens { get; set; }
    }
}
