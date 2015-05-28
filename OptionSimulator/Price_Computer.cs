using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionSimulator
{
    class Price_Computer
    {
        public static bool compute_price(Vector<double> payoffs, double r, double T, out double price, out double err)
        {
            // price is average of payoffs
            // error is calculated likewise
            int n = payoffs.Count;
            Vector<double> prices = Math.Exp(-r * T) * payoffs;
            price = prices.Average(); 
            err = Math.Sqrt((prices.PointwiseMultiply(prices).Average() - price*price)/(n - 1));
            return true;
        }
    }
}
