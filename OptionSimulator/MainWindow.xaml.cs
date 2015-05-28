using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OptionSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static int NUM_COMPUTE_THREADS = 20;
        private ObservableCollection<Stock> stocks;
        private GraphModel graphModel;
        private string put_call;

        public MainWindow()
        {
            InitializeComponent();
            graphModel = new GraphModel();
            Plot1.Model = graphModel.PlotModel;

            List<IStock_Simulator> simulators = new List<IStock_Simulator>();
            simulators.Add(new Geometric_Brownian_Motion_Simulator());
            simulators.Add(new HestonModel());
            stock_price_model_select.ItemsSource = simulators;

            stocks = new ObservableCollection<Stock>();
            stocks.Add(new Stock("S1", 50.0, 0.2));
            stock_list.ItemsSource = stocks;
        }

        private void calculate_btn_Click(object sender, RoutedEventArgs e)
        {
            string payoff_eqn;
            int num_samples, intervals;
            double r, T, strike_price;
            Dictionary<string, double> extra_params = new Dictionary<string,double>();
            IStock_Simulator simulator;
            if (!getAllInputParams(out num_samples, out intervals, out r, out T, 
                out strike_price, out simulator, out payoff_eqn, extra_params))
                return;

            num_samples -= num_samples % NUM_COMPUTE_THREADS;
            foreach (Stock stock in stocks)
                stock.initialize_matrix(num_samples, intervals);
            // set up cov matrix

            DateTime start_time = DateTime.Now;
            
            // simulate stock(s) movements
            simulator.simulate_stock(stocks, null, r, T, intervals, num_samples, extra_params);

            // plot 5 paths of first stock
            Stock stock1 = stocks[0];
            List<Vector<double>> pathsToPlot = new List<Vector<double>>();
            for (int i = 0; i < 5; ++i)
                pathsToPlot.Add(stock1.price_paths.Row(i));
            graphModel.LoadData(pathsToPlot, stock1.InitialPrice);
            graphModel.PlotModel.InvalidatePlot(true);

            // compute payoff/SE (by "plugging in" values for stock)
            double price, err;
            Vector<double> payoffs;
            Dictionary<string, Stock> stock_dict = new Dictionary<string, Stock>();
            foreach (Stock stock in stocks)
                stock_dict[stock.Name] = stock;

            Dictionary<string, double> constants = new Dictionary<string, double>();
            constants["K"] = strike_price;

            if (payoff_eqn == "")
                Payoff_Computer.compute_payoff(stock_dict, num_samples, put_call, strike_price, out payoffs);
            else
            {
                try
                {
                    Payoff_Computer.compute_custom_payoff(payoff_eqn, stock_dict, num_samples, constants, out payoffs);
                }
                catch (Exception exp)
                {
                    err_lbl.Content = exp.Message;
                    return;
                }
            }
            
            Price_Computer.compute_price(payoffs, r, T, out price, out err);
            price_lbl.Content = price.ToString();
            std_err_lbl.Content = err.ToString();
            err_lbl.Content = "Took " + (DateTime.Now - start_time).TotalMilliseconds.ToString() + "ms";
        }

        private void add_stock_btn_Click(object sender, RoutedEventArgs e)
        {
            string name = stock_name_txt.Text;
            double volatility;
            bool res = Double.TryParse(vol_txt.Text, out volatility);
            if (res == false)
            {
                err_lbl.Content = "Couldn't parse: " + vol_txt.Text;
                return;
            }

            double initial_price;
            res = Double.TryParse(init_price_txt.Text, out initial_price);
            if (res == false)
            {
                err_lbl.Content = "Couldn't parse: " + init_price_txt.Text;
                return;
            }

            // should allow editing of stock
            stocks.Add(new Stock(name, initial_price, volatility));
            stock_list.ItemsSource = stocks;
            err_lbl.Content = "";
        }

        private bool getAllInputParams(out int num_samples, out int intervals,
            out double r, out double T, out double strike_price,
            out IStock_Simulator simulator, out string payoff_eqn, 
            Dictionary<string, double> extra_params)
        {
            num_samples = 100;
            intervals = 1;
            r = 0.05;
            T = 1.0;
            strike_price = 50.0;
            simulator = null;

            payoff_eqn = custom_payoff_txt.Text;

            bool res = Int32.TryParse(samples_txt.Text, out num_samples);
            if (res == false)
            {
                err_lbl.Content = "Couldn't parse number of samples";
                return false;
            }

            res = Int32.TryParse(intervals_txt.Text, out intervals);
            if (res == false)
            {
                err_lbl.Content = "Couldn't parse number of intervals";
                return false;
            }

            res = Double.TryParse(interest_rate_txt.Text, out r);
            if (res == false)
            {
                err_lbl.Content = "Couldn't parse interest rate";
                return false;
            }

            res = Double.TryParse(T_txt.Text, out T);
            if (res == false)
            {
                err_lbl.Content = "Couldn't parse T";
                return false;
            }

            res = Double.TryParse(strike_txt.Text, out strike_price);
            if (res == false)
            {
                err_lbl.Content = "Couldn't parse strike price";
                return false;
            }

            if (theta_txt.IsEnabled)
            {
                double theta;
                res = Double.TryParse(theta_txt.Text, out theta);
                if (res == false)
                {
                    err_lbl.Content = "Couldn't parse theta";
                    return false;
                }
                extra_params["theta"] = theta;

                double long_var;
                res = Double.TryParse(longvar_txt.Text, out long_var);
                if (res == false)
                {
                    err_lbl.Content = "Couldn't parse long_var";
                    return false;
                }
                extra_params["long_var"] = long_var;

                double eps;
                res = Double.TryParse(eps_txt.Text, out eps);
                if (res == false)
                {
                    err_lbl.Content = "Couldn't parse eps";
                    return false;
                }
                extra_params["eps"] = eps;
            }

            if (stock_price_model_select.SelectedIndex == -1)
            {
                err_lbl.Content = "No model selected";
                return false;
            }
            simulator = (IStock_Simulator)stock_price_model_select.SelectedItem;
            return true;
        }

        private void stock_price_model_select_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IStock_Simulator sim = e.AddedItems[0] as IStock_Simulator;
            if (sim != null)
            {
                sim.EnableInput(this);
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            this.put_call = (sender as RadioButton).Content.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            WrapPanel parent = (button.Parent as WrapPanel);
            string temp_name = "";
            double temp_price = 0.0, temp_vol = 0.0;
            bool res;

            if (button.Tag as string != "not editing")
            {
                // disable editing and update stock properties
                foreach (FrameworkElement elt in parent.Children)
                {
                    string tag = elt.Tag as string;
                    switch (tag)
                    {
                        case "Name":
                            temp_name = (elt as TextBox).Text;
                            break;
                        case "Price":
                            res = Double.TryParse((elt as TextBox).Text, out temp_price);
                            if (res == false)
                            {
                                err_lbl.Content = "Couldn't parse price";
                                return;
                            }
                            break;
                        case "Vol":
                            res = Double.TryParse((elt as TextBox).Text, out temp_vol);
                            if (res == false)
                            {
                                err_lbl.Content = "Couldn't parse price";
                                return;
                            }
                            break;
                    }
                    if (tag == "Name" || tag == "Price" || tag == "Vol")
                        elt.IsEnabled = false;
                }

                // update properties
                Stock stock = stocks.First((s) => s.Name == (button.Tag as string));
                stock.Name = temp_name;
                stock.InitialPrice = temp_price;
                stock.Sig = temp_vol;
                button.Content = "Edit";
                button.Tag = "not editing";
            }
            else
            {
                // enable editing
                foreach (FrameworkElement elt in parent.Children)
                {
                    string tag = elt.Tag as string;
                    if (tag == "Name")
                        temp_name = (elt as TextBox).Text;
                    if (tag == "Name" || tag == "Price" || tag == "Vol")
                        elt.IsEnabled = true;
                }
                button.Content = "Save Changes";
                button.Tag = temp_name;
            }
        }
    }
}
