using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class FinalizarCompraView
    {
        public int idCompra { get; set; }

        [BindNever]
        public double subtotal { get; set; }

        [BindNever]
        public double total { get; set; }

        [Required(ErrorMessage = "Nome obrigatório")]
        public string nome { get; set; }

        [Required(ErrorMessage = "NIF Obrigatório")]
        public string nif { get; set; }

        [Required(ErrorMessage = "Email obrigatório")]
        public string email { get; set; }

        [Required(ErrorMessage = "Nº telemóvel obrigatório")]
        public string tlm { get; set; }

        [Required(ErrorMessage = "Morada obrigatória")]
        public string morada { get; set; }

    }
}
