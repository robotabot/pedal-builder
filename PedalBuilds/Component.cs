using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedalBuilds
{
    class Component
    {
        public Int64 Id { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Notes { get; set; }
        public string Url { get; set; }
        public double Price { get; set; }

        public Component(string type, string value, string notes, string url, double price, Int64 id)
        {
            Id = id;
            Type = type;
            Value = value;
            Notes = notes;
            Url = url;
            Price = price;
        }
        
    }
}
