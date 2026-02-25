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

        public string SimulationUrl { get; set; } = "http://127.0.0.1";

        public string URL { get; set; } = "http://192.168.253.128";
    }
}
