using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionSimulator
{
    interface IStock_Simulator
    {
        bool simulate_stock(Collection<Stock> stocks, Matrix<double> cov, double r, double T,
            int intervals, int num_samples, Dictionary<string, double> extra_params);

        void EnableInput(MainWindow mainWindow);
    }
}
