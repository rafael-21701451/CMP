using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class ContactosView
    {
        [Required(ErrorMessage ="Nome obrigatório")]
        public string nome { get; set; }

        [Required(ErrorMessage = "Email obrigatório")]
        public string email { get; set; }

        [Required(ErrorMessage = "Empresa obrigatória")]
        public string empresa { get; set; }

        [Required(ErrorMessage = "Setor obrigatório")]
        public string setor { get; set; }

        [Required(ErrorMessage = "Mensagem obrigatória")]
        public string mensagem { get; set; }


    }
}
