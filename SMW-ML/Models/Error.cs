using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Models
{
    public class Error
    {
        public Error()
        {
            FieldError = "";
            Description = "";
        }

        public string FieldError { get; set; }
        
        public string Description { get; set; }
    }
}
