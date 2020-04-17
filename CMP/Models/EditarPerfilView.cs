using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class EditarPerfilView
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Nome obrigatório")]
        public string nome { get; set; }

        [Required(ErrorMessage = "Username obrigatório")]
        public string username { get; set; }

        [Required(ErrorMessage = "Email obrigatório")]
        public string email { get; set; }

        [Required(ErrorMessage = "Password obrigatória")]
        public string password { get; set; }

        [Required(ErrorMessage = "Password obrigatória")]
        public string passwordCmf { get; set; }

    }
}
