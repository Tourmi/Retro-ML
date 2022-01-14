using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Neural.Training
{
    /// <summary>
    /// Adapter class that allows training a neural network
    /// </summary>
    public interface INeuralTrainer
    {
        bool IsTraining { get; }
        void StartTraining(string configPath);
        void StopTraining();
    }
}
