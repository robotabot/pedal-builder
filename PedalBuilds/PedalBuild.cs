using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedalBuilds
{
    class PedalBuild
    {
        public string Name { get; set; }
        public Int64 PedalId { get; set; }
        public Decimal Price { get; set; }
        public Int16 Quantity { get; set; }

        public PedalBuild(string name, Int64 pedalId, Decimal price, Int16 quantity)
        {
            Name = name;
            PedalId = pedalId;
            Price = price;
            Quantity = quantity;
        }
    }
}
