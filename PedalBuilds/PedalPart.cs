using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedalBuilds
{
    class PedalPart
    {
        public Int64 id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public Int64 ComponentId { get; set; }
        public Int64 PedalId { get; set; }
        public Decimal Price { get; set; }

    }
}
