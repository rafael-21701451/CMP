﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class ProjetoAtribuido
    {
        public int id { get; set; }
        public string produto { get; set; }
        public string nomeProdutor { get; set; }
        public string estado { get; set; }
        public string categoriaProduto { get; set; }

        public int briefingId { get; set; }
    }
}
