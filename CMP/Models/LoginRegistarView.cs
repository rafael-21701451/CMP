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

        public string nome { get; set; }

        public string morada { get; set; }

        public string username { get; set; }
        
        public string emailReg { get; set; }

        public string passwordReg { get; set; }

    }

    }
