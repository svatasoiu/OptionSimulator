using MathNet.Numerics.LinearAlgebra;
using NCalc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OptionSimulator
{
    class Payoff_Computer
    {
        public static bool compute_custom_payoff(string payoff_eqn, Dictionary<string, Stock> stocks, 
            int num_samples, Dictionary<string, double> constants, out Vector<double> payoffs)
        {
            // need to parse/use regex to make payoff_eqn into executable expression
            // payoff_eqn is something like S1-K or S1-S2
            // find all strings
            Regex r = new Regex("[a-zA-Z_]+[0-9]*");
            string payoff_replaced = r.Replace(payoff_eqn, (match) =>
            {
                // don't match Max/avg/geom_avg
                string word = match.Value;
                switch (word)
                {
                    case "Max":
                    case "Avg":
                    case "Geom_avg":
                        return word;
                    default:
                        if (constants.ContainsKey(word))
                            return constants[word].ToString();
                        else
                        {
                            if (stocks.ContainsKey(word))
                                return "[" + word + "]";
                            else
                                throw new EvaluationException("Couldn't parse: " + word);
                        }
                }
            });
            Console.WriteLine(payoff_eqn);
            Console.WriteLine(payoff_replaced);

            // call option for now
            Dictionary<string, Vector<double>> prices = new Dictionary<string, Vector<double>>();
            foreach (Stock stock in stocks.Values)
                prices[stock.Name] = stock.final_prices;

            int j = 0;
            // very expensive to make new expression every time...
            Expression e = new Expression(payoff_replaced);

            e.EvaluateParameter += delegate(string name, ParameterArgs args)
            {
                args.Result = prices[name][j];
            };

            e.EvaluateFunction += delegate(string name, FunctionArgs args)
            {
                switch (name)
                {
                    case "Max":
                        double p1, p2;
                        var arg1 = args.Parameters[0].Evaluate();
                        var arg2 = args.Parameters[1].Evaluate();
                        try { p1 = (double)arg1; }
                        catch (InvalidCastException _) { p1 = (int)arg1; }
                        try { p2 = (double)arg2; }
                        catch (InvalidCastException _) { p2 = (int)arg2; }

                        args.Result = Math.Max(p1, p2);
                        break;
                    case "Avg":
                        string parsed = args.Parameters[0].ParsedExpression.ToString();
                        args.Result = stocks[parsed.Substring(1, parsed.Length - 2)].price_paths.Row(j).Average();
                        break;
                    case "Geom_avg":
                        break;
                    default:
                        break;
                }
            };

            payoffs = Vector<double>.Build.Dense(num_samples, (i) =>
            {
                j = i;
                return (double)e.Evaluate(); // Math.Max((double)e.Evaluate(), 0);
            });
            return true;
        }

        public static bool compute_payoff(Dictionary<string, Stock> stocks, int num_samples, string put_call, 
            double strike_price, out Vector<double> payoffs)
        {
            Vector<double> prices = stocks["S1"].final_prices;
            int pc = 1;
            if (put_call == "Put")
                pc = -1;

            payoffs = Vector<double>.Build.Dense(num_samples, (i) => Math.Max(pc * (prices[i] - strike_price), 0));
            return true;
        }
    }
}
