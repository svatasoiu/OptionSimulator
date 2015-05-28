using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptionSimulator
{
    class Stock
    {
        public Stock(string n, double init, double vol)
        {
            this.Name = n;
            this.InitialPrice = init;
            this.Sig = vol;
        }

        public void initialize_matrix(int num_samples, int intervals)
        {
            this.N = num_samples;
            this.intervals = intervals;
            this.price_paths = null;
            this.price_paths = Matrix<double>.Build.Dense(num_samples, intervals);
        }

        public string Name { get; set; }
        public double InitialPrice { get; set; }
        public double Sig { get; set; }
        public Matrix<double> price_paths { get; set; }
        public Vector<double> final_prices 
        {
            get 
            {
                return this.price_paths.Column(intervals - 1);
            } 
        }

        public int N { get; set; }

        public int intervals { get; set; }
    }
}
