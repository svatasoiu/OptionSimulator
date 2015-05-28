using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OptionSimulator
{
    class Geometric_Brownian_Motion_Simulator : IStock_Simulator
    {
        int NUM_THREADS = MainWindow.NUM_COMPUTE_THREADS;

        public bool simulate_stock(Collection<Stock> stocks, Matrix<double> cov, double r, double T,
            int intervals, int num_samples, Dictionary<string, double> extra_params)
        {
            Matrix<double> C = null;
            if (cov != null)
                C = cov.Cholesky().Factor;
              
            ManualResetEvent[] resetEvents = new ManualResetEvent[NUM_THREADS];
            
            int num_per_thread = num_samples / NUM_THREADS;

            // use thread pool
            for (int thread_no = 0; thread_no < NUM_THREADS; ++thread_no)
            {
                resetEvents[thread_no] = new ManualResetEvent(false);
                BrownianCompute b = new BrownianCompute(stocks, C, r, T, intervals, num_per_thread, resetEvents[thread_no]);
                ThreadPool.QueueUserWorkItem(b.ThreadPoolCallback, thread_no);
            }

            foreach (var e in resetEvents)
                e.WaitOne();
            Console.WriteLine("All calculations are complete.");
            return true;
        }

        private class BrownianCompute
        {
            private Collection<Stock> _stocks;
            private Matrix<double> _C;
            private double _r;
            private double _T;
            private int _intervals;
            private int _num_per_thread;
            private ManualResetEvent _doneEvent;

            // Constructor. 
            public BrownianCompute(Collection<Stock> stocks, Matrix<double> C, double r, double T,
                int intervals, int num_per_thread, ManualResetEvent doneEvent)
            {
                _stocks = stocks;
                _C = C;
                _r = r;
                _T = T;
                _intervals = intervals;
                _num_per_thread = num_per_thread;
                _doneEvent = doneEvent;
            }

            // Wrapper method for use with thread pool. 
            public void ThreadPoolCallback(Object threadContext)
            {
                int threadIndex = (int) threadContext;
                Console.WriteLine("thread {0} started...", threadIndex);
                for (int i = 0; i < _num_per_thread; ++i)
                {
                    //Matrix<double> Z = Matrix<double>.Build.Random(stocks.Count, intervals);
                    Matrix<double> Z = Matrix<double>.Build.Random(_intervals, _stocks.Count);
                    Matrix<double> Y;
                    if (_C == null)
                        Y = Z;
                    else
                        Y = _C * Z;

                    for (int j = 0; j < _stocks.Count; ++j)
                        compute_path(_stocks[j], j, _intervals, _num_per_thread*threadIndex + i, _r, Y, _T);
                }
                Console.WriteLine("thread {0} result calculated...", threadIndex);
                _doneEvent.Set();
            }
        }

        private static object compute_path(Stock stock, int stock_index, int intervals, int sample_num, double r, Matrix<double> Y, double T)
        {
            double log_price = Math.Log(stock.InitialPrice);
            double dt = T / intervals;
            double c1 = (r - stock.Sig * stock.Sig / 2.0) * dt;
            double c2 = Math.Sqrt(dt) * stock.Sig;
            Vector<double> col = Y.Column(stock_index);
            Vector<double> log_prices = Vector<double>.Build.Dense(intervals);
            for (int i = 0; i < intervals; ++i)
            {
                log_price += c1 + c2 * col[i]; // not just Y
                log_prices[i] = log_price;
            }
            stock.price_paths.SetRow(sample_num, log_prices.PointwiseExp());
            return null;
        }

        public void EnableInput(MainWindow mainWindow)
        {
            mainWindow.theta_txt.IsEnabled = false;
            mainWindow.longvar_txt.IsEnabled = false;
            mainWindow.eps_txt.IsEnabled = false;
        }
    }
}
