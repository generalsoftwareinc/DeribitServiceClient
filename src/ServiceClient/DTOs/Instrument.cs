using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ServiceClient.DTOs
{
    public class InstrumentConfiguration
    {
        public string InstrumentName { get; set; } = string.Empty;
        public string TickerInterval { get; set; } = string.Empty;
        public string BookInterval { get; set; } = string.Empty;
    }
}
