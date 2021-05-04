using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionApp1.Models
{
    public class TodoUpdateModel
    {
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }
}
