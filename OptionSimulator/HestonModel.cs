using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionSimulator
{
    class HestonModel : IStock_Simulator
    {
        public bool simulate_stock(Collection<Stock> stocks, Matrix<double> cov, 
            double r, double T, int intervals, int num_samples,
            Dictionary<string, double> extra_params)
        {
            Matrix<double> C = Matrix<double>.Build.DiagonalIdentity(stocks.Count);
            if (cov != null)
                C = cov.Cholesky().Factor;

            for (int i = 0; i < num_samples; ++i)
            {
                Matrix<double> Z = Matrix<double>.Build.Random(stocks.Count, intervals);
                Matrix<double> Y;
                if (cov == null)
                    Y = Z;
                else
                    Y = C * Z;

                for (int j = 0; j < stocks.Count; ++j)
                    compute_path(stocks[j], j, intervals, i, r, Y, T, extra_params);
            }
            return true;
        }

        private static object compute_path(Stock stock, int stock_index, int intervals,
            int sample_num, double r, Matrix<double> Y, double T, Dictionary<string, double> extra_params)
        {
            // do euler scheme to compute volatility
            double theta = extra_params["theta"];
            double long_var = extra_params["long_var"];
            double eps = extra_params["eps"];
            double dt = T / intervals;
            Vector<double> Z = Vector<double>.Build.Random(intervals);
            Vector<double> volatilities = Vector<double>.Build.Dense(intervals);
            double vol = stock.Sig;
            volatilities[0] = vol;
            for (int i = 0; i < intervals - 1; ++i) {
                double v = Math.Max(vol,0);
                vol += theta * (long_var - v) * dt + eps * Math.Sqrt(v*dt) * Z[i];
                volatilities[i+1] = vol;
            }

            // do euler scheme to compute stock price
            double price = stock.InitialPrice;
            for (int i = 0; i < intervals; ++i)
            {
                double v = Math.Max(volatilities[i], 0);
                price *= Math.Exp((r - v / 2.0) * dt + Math.Sqrt(dt * v) * Y[stock_index, i]); // not just Y
                stock.price_paths[sample_num, i] = price;
            }
            return null;
        }

        public void EnableInput(MainWindow mainWindow)
        {
            mainWindow.theta_txt.IsEnabled = true;
            mainWindow.longvar_txt.IsEnabled = true;
            mainWindow.eps_txt.IsEnabled = true;
        }
    }
}
