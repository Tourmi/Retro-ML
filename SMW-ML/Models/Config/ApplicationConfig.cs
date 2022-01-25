using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Models.Config
{
    public class ApplicationConfig
    {
        public ApplicationConfig()
        {
            AIObjective = "";
            StopTrainingCondition = "";
            ArduinoCommunicationPort = "COM3";
        }

        public int Multithread { get; set; }
        public string ArduinoCommunicationPort { get; set; }
        public string AIObjective { get; set; }
        public string StopTrainingCondition { get; set; }
        public int? StopTrainingConditionValue { get; set; }
    }
}
