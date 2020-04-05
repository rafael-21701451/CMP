using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class Briefing
    {
        public int id { get; set; }

        public string empresa { get; set; }

        public string setor { get; set; }

        public string historia_empresa { get; set; }

        public string objetivo_negocio { get; set; }

        public string estrategia { get; set; }

        public string produtos_comercializados { get; set; }

        public string marca { get; set; }

        public string imagem_corporativa { get; set; }

        public string posicionamento { get; set; }

        public string publico_alvo { get; set; }

        public string concorrentes { get; set; }

        public string objetivos { get; set; }

        public string resultados_esperados { get; set; }

        public string permissas { get; set; }

        public string restricoes { get; set; }

        public DateTime data_entrega { get; set; }

        public DateTime cronograma_1 { get; set; }

        public DateTime cronograma_2 { get; set; }

        public DateTime cronograma_3 { get; set; }

        public string linha_seguir { get; set; }

        public string tom_voz { get; set; }

        public string tipo_letra { get; set; }

        public string cor { get; set; }

    }
}
