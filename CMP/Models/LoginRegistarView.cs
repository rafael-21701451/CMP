using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class LoginRegistarView
    {
        public string email { get; set; }

        public string password { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        public string nome { get; set; }

        [Required(ErrorMessage = "Morada é obrigatória")]
        public string morada { get; set; }

        [Required(ErrorMessage = "Username é obrigatório")]
        public string username { get; set; }
        

        [Required(ErrorMessage = "Email é obrigatório")]
        public string emailReg { get; set; }

        [Required(ErrorMessage = "Password é obrigatória")]
        public string passwordReg { get; set; }

    }

    }
