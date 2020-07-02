using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class MensagemCM
    {
        public int id { get; set; }
        public String assunto { get; set; }
        public String remetente { get; set; }
        public String textoMensagem { get; set; }
        public string data { get; set; }

    }
}
