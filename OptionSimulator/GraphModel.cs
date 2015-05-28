using MathNet.Numerics.LinearAlgebra;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionSimulator
{
    class GraphModel : INotifyPropertyChanged
    {
        private PlotModel plotModel;
        public PlotModel PlotModel
        {
            get { return plotModel; }
            set { plotModel = value; OnPropertyChanged("PlotModel"); }
        }

        public GraphModel()
        {
            PlotModel = new PlotModel();
            SetUpModel();
        }
 
        public event PropertyChangedEventHandler PropertyChanged;
 
        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetUpModel()
        {
            PlotModel.LegendTitle = "Legend";
            PlotModel.LegendOrientation = LegendOrientation.Horizontal;
            PlotModel.LegendPlacement = LegendPlacement.Outside;
            PlotModel.LegendPosition = LegendPosition.TopRight;
            PlotModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            PlotModel.LegendBorder = OxyColors.Black;

            PlotModel.Title = "5 Sample Paths";

            var timeAxis = new LinearAxis(AxisPosition.Bottom)
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = "Time"
            };
            PlotModel.Axes.Add(timeAxis);

            var valueAxis = new LinearAxis(AxisPosition.Left) { 
                MajorGridlineStyle = LineStyle.Solid, 
                MinorGridlineStyle = LineStyle.Dot, 
                Title = "Stock Price ($)" 
            };
            PlotModel.Axes.Add(valueAxis);
        }

        public void LoadData(List<Vector<double>> paths, double initial_value)
        {
            PlotModel.Series.Clear();
            int i = 0;
            foreach (Vector<double> path in paths)
            {
                var lineSeries = new LineSeries
                {
                    StrokeThickness = 2,
                    MarkerSize = 3,
                    MarkerStroke = OxyColor.FromRgb(255, 0, 0),
                    MarkerType = MarkerType.Circle,
                    CanTrackerInterpolatePoints = false,
                    Title = string.Format("Simulation {0}", i++),
                    Smooth = false,
                };

                // init value
                lineSeries.Points.Add(new DataPoint(0, initial_value));

                foreach (var v in path.EnumerateIndexed())
                    lineSeries.Points.Add(new DataPoint(v.Item1 + 1, v.Item2));

                PlotModel.Series.Add(lineSeries);
            }
        }
    }
}
