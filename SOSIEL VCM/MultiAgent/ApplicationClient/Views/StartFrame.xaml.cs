using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ApplicationClient.ViewModel;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using RDotNet;

namespace ApplicationClient
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StartFrame : Page
    {
        private MainViewModel viewModel;

        /// <summary>
        ///     Initializes a new instance of the MainWindow class.
        /// </summary>
        public StartFrame()
        {
            InitializeComponent();
        }

        private void StartFrame_OnLoaded(object sender, RoutedEventArgs e)
        {
            viewModel = DataContext as MainViewModel;

            viewModel.UpdateD3 += viewModel_UpdateD3;
        }

        private void viewModel_UpdateD3(object sender, EventArgs args)
        {
            var random = new Random(256);

            var engine = REngine.GetInstance();

            var sequence = engine.Evaluate("x <- seq(" + 1 + ", " + viewModel.RParameter + ", 1)").AsNumeric();

            foreach (var agent in viewModel.Contributions)
            {
                var strBuilder = new StringBuilder();
                foreach (var contribution in agent.Data)
                {
                    strBuilder.Append(contribution + ",");
                }
                strBuilder.Length--;
                var dnorm = engine.Evaluate("y <- c(" + strBuilder + ")").AsNumeric();
                var data = sequence.Zip(dnorm, (x, y) => new Point(x, y));

                var contributions = new ObservableDataSource<Point>();
                contributions.AppendMany(data);

                chartPlotter.AddLineGraph(contributions,
                    Color.FromRgb((byte) random.Next(50), (byte) random.Next(50,100), (byte) random.Next(100,256)),2, agent.Name);
            }
        }
    }
}