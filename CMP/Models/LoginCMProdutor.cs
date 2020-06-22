using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Models
{
    public class LoginCMProdutor
    {

        public bool sessaoCM { get; set; }

        public bool sessaoProdutor { get; set; }

        public string emailCM { get; set; }

        public string passwordCM { get; set; }

        public string emailProducer { get; set; }

        public string passwordProducer { get; set; }
    }

    }
