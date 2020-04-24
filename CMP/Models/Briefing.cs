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

        [MaxLength(255, ErrorMessage = "Limite de 255 caracteres.")]
        [Required(ErrorMessage = "Empresa obrigatória")]
        public string empresa { get; set; }

        [MaxLength(255, ErrorMessage = "Limite de 255 caracteres.")]
        [Required(ErrorMessage = "Setor obrigatório")]
        public string setor { get; set; }

        [MaxLength(512, ErrorMessage = "Limite de 512 caracteres.")]
        [Required(ErrorMessage = "História obrigatória")]
        public string historia_empresa { get; set; }

        [MaxLength(255, ErrorMessage = "Limite de 255 caracteres.")]
        [Required(ErrorMessage = "Objetivo obrigatório")]
        public string objetivo_negocio { get; set; }

        [MaxLength(255, ErrorMessage = "Limite de 255 caracteres.")]
        [Required(ErrorMessage = "Estratégia obrigatória")]
        public string estrategia { get; set; }

        [MaxLength(255, ErrorMessage = "Limite de 255 caracteres.")]
        [Required(ErrorMessage = "Produtos obrigatória«os")]
        public string produtos_comercializados { get; set; }

        [MaxLength(255, ErrorMessage = "Limite de 255 caracteres.")]
        [Required(ErrorMessage = "Marca obrigatória")]
        public string marca { get; set; }

        [MaxLength(50, ErrorMessage = "Limite de 50 caracteres.")]
        [Required(ErrorMessage = "Imagem obrigatória")]
        public string imagem_corporativa { get; set; }

        [MaxLength(255, ErrorMessage = "Limite de 255 caracteres.")]
        [Required(ErrorMessage = "Posicionamento obrigatório")]
        public string posicionamento { get; set; }

        [MaxLength(255, ErrorMessage = "Limite de 255 caracteres.")]
        [Required(ErrorMessage = "Público obrigatória")]
        public string publico_alvo { get; set; }

        [MaxLength(255, ErrorMessage = "Limite de 255 caracteres.")]
        [Required(ErrorMessage = "Concorrentes obrigatórios")]
        public string concorrentes { get; set; }

        [MaxLength(512, ErrorMessage = "Limite de 512 caracteres.")]
        [Required(ErrorMessage = "Objetivos obrigatórios")]
        public string objetivos { get; set; }

        [MaxLength(255, ErrorMessage = "Limite de 255 caracteres.")]
        [Required(ErrorMessage = "Resultados obrigatórios")]
        public string resultados_esperados { get; set; }

        [MaxLength(255, ErrorMessage = "Limite de 255 caracteres.")]
        [Required(ErrorMessage = "Permissas obrigatórias")]
        public string permissas { get; set; }

        [MaxLength(255, ErrorMessage = "Limite de 255 caracteres.")]
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

        [MaxLength(512, ErrorMessage = "Limite de 512 caracteres.")]
        public string linha_seguir { get; set; }

        [MaxLength(255, ErrorMessage = "Limite de 255 caracteres.")]
        public string tom_voz { get; set; }

        public string tipo_letra { get; set; }

        public string cor { get; set; }

    }
}
