using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using ApplicationClient.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Win32;
using MultiAgent;
using RDotNet;

namespace ApplicationClient.ViewModel
{
    /// <summary>
    ///     This class contains properties that the main View can data bind to.
    ///     <para>
    ///         See http://www.mvvmlight.net
    ///     </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly FrameNavigationService _navigator;
        private int _iterations;
        private double _mParameter;
        private MultiAgentSystem _multiSystemAgent;
        private int _nParameter;
        private int _runIteration;

        public MainViewModel()
        {
            _navigator = (FrameNavigationService)ServiceLocator.Current.GetInstance<INavigationService>();

            Updates = new ObservableCollection<string>();

            Func<double, double> cooperationFunc = averageCotribution => averageCotribution >= 5 ? 10 : 0;
            Func<double, double> trendFunc = averageCotribution => averageCotribution;
            Func<double, double> freeRiderFunc = averageCotribution => 0;

            _multiSystemAgent = new MultiAgentSystem(cooperationFunc, trendFunc, freeRiderFunc);

            NParameter = 1;
            Iterations = 1;
        }

        public string ConfigurationFilePath { get; set; }

        public double MParameter
        {
            get { return _mParameter; }
            set
            {
                if (value < 0)
                    return;
                _mParameter = value;
                RaisePropertyChanged();
            }
        }

        public int Iterations
        {
            get { return _iterations; }
            set
            {
                _iterations = value < 1 ? 1 : value;
                RaisePropertyChanged();
            }
        }

        public int NParameter
        {
            get { return _nParameter; }
            set
            {
                _nParameter = value < 1 ? 1 : value;
                _multiSystemAgent.InititalizeAgents(value);
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> Updates { get; set; }

        public RelayCommand CommunicationMapCommand
        {
            get { return new RelayCommand(CommunicationMap); }
        }

        public RelayCommand RunAllCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _multiSystemAgent.RunService(Iterations - _runIteration, MParameter, UpdatingStatus);
                    _runIteration = Iterations;
                    UpdateContributions();
                });
            }
        }

        public RelayCommand RunOnceCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (++_runIteration > Iterations)
                        return;
                    _multiSystemAgent.RunServiceOnce(++_runIteration, MParameter, UpdatingStatus);
                    UpdateContributions();
                });
            }
        }

        public RelayCommand SetConfigurationFilePathCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var openFileDialog = new OpenFileDialog
                    {
                        Filter = "XML Files (.xml)|*.xml|All Files (*.*)|*.*",
                        FilterIndex = 1
                    };

                    var okResult = openFileDialog.ShowDialog();

                    if (okResult == true)
                    {
                        ConfigurationFilePath = openFileDialog.FileName;
                    }
                });
            }
        }

        public ObservableDataSource<Point> Contributions { get; set; }

        public RelayCommand AgentsCommand
        {
            get { return new RelayCommand(AgentsBuilder); }
        }

        private void UpdateContributions()
        {
            Contributions = new ObservableDataSource<Point>();

            REngine engine = REngine.GetInstance();

            NumericVector sequence = engine.Evaluate("x <- seq(" + 1 + ", " + Iterations + ", 1)").AsNumeric();

            foreach (var agent in _multiSystemAgent.Agents)
            {
                StringBuilder strBuilder = new StringBuilder();
                foreach (var contribution in agent.Contributions)
                {
                    strBuilder.Append(contribution+",");
                }
                strBuilder.Length--;
                NumericVector dnorm = engine.Evaluate("y <- c(" + strBuilder + ")").AsNumeric();
                IEnumerable<Point> data = sequence.Zip(dnorm, (x, y) => new Point(x, y));
                Contributions.AppendMany(data);
            }

            RaisePropertyChanged("Contributions");
        }

        private void UpdatingStatus(string obj)
        {
            Updates.Add(obj);
        }

        private void AgentsBuilder()
        {
            _navigator.NavigateTo("AgentsBuilder");

            MessengerInstance.Send(_multiSystemAgent.Agents);
        }

        private void CommunicationMap()
        {
            _navigator.NavigateTo("CommunicationMap", _multiSystemAgent.Agents.Count);
            MessengerInstance.Send(_multiSystemAgent.Agents.Count);
        }
    }
}