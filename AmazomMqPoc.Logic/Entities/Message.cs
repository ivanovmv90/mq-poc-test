using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazomMqPoc.Logic.Entities
{
    public class Message
    {
        public string Id { get; set; }
        public string JobId { get; set; }
        public Dictionary<string, string> CustomHeaders { get; set; }
        public string Body { get; set; }

        public Schedule Schedule { get; set; }
    }
}
