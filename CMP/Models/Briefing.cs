using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class Briefing
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Empresa obrigatória")]
        public string empresa { get; set; }

        [Required(ErrorMessage = "Setor obrigatório")]
        public string setor { get; set; }

        [Required(ErrorMessage = "História obrigatória")]
        public string historia_empresa { get; set; }

        [Required(ErrorMessage = "Objetivo obrigatório")]
        public string objetivo_negocio { get; set; }

        [Required(ErrorMessage = "Estratégia obrigatória")]
        public string estrategia { get; set; }

        [Required(ErrorMessage = "Produtos obrigatória«os")]
        public string produtos_comercializados { get; set; }

        [Required(ErrorMessage = "Marca obrigatória")]
        public string marca { get; set; }

        [Required(ErrorMessage = "Imagem obrigatória")]
        public string imagem_corporativa { get; set; }

        [Required(ErrorMessage = "Posicionamento obrigatório")]
        public string posicionamento { get; set; }

        [Required(ErrorMessage = "Público obrigatória")]
        public string publico_alvo { get; set; }

        [Required(ErrorMessage = "Concorrentes obrigatórios")]
        public string concorrentes { get; set; }

        [Required(ErrorMessage = "Objetivos obrigatórios")]
        public string objetivos { get; set; }

        [Required(ErrorMessage = "Resultados obrigatórios")]
        public string resultados_esperados { get; set; }

        [Required(ErrorMessage = "Permissas obrigatórias")]
        public string permissas { get; set; }

        [Required(ErrorMessage = "Restrições obrigatórias")]
        public string restricoes { get; set; }

        [Required(ErrorMessage = "Data obrigatória")]
        public DateTime? data_entrega { get; set; }

        [Required(ErrorMessage = "Data obrigatória")]
        public DateTime? cronograma_1 { get; set; }

        [Required(ErrorMessage = "Data obrigatória")]
        public DateTime? cronograma_2 { get; set; }

        [Required(ErrorMessage = "Data obrigatória")]
        public DateTime? cronograma_3 { get; set; }

        public string linha_seguir { get; set; }

        public string tom_voz { get; set; }

        public string tipo_letra { get; set; }

        public string cor { get; set; }

    }
}
