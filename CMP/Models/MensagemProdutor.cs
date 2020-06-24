using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class MensagemProdutor
    {
        public int id { get; set; }
        public String assunto { get; set; }

        public String remetente { get; set; }
    }
}
