using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbpOverallAuth.Configuration.ThirdParty
{
    public class TM
    {
        public bool IsSimulation { get; set; }

        public string SimulationUrl { get; set; } = "192.168.253.128";

        public int SimulationPort { get; set; }

        public string URL { get; set; } = "192.168.253.128";

        public int Port { get; set; } 
    }
}
