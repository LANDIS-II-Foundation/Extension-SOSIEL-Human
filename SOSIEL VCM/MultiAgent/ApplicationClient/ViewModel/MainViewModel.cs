using System;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using ApplicationClient.Services;
using ChartSurfaceControl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Win32;
using MultiAgent;

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
            _navigator = (FrameNavigationService) ServiceLocator.Current.GetInstance<INavigationService>();

            Updates = new ObservableCollection<string>();

            Func<double, double> cooperationFunc = averageCotribution => averageCotribution >= 5 ? 10 : 0;
            Func<double, double> trendFunc = averageCotribution => averageCotribution;
            Func<double, double> freeRiderFunc = averageCotribution => 0;

            _multiSystemAgent = new MultiAgentSystem(cooperationFunc, trendFunc, freeRiderFunc);
        }

        public string ConfigurationFilePath { get; set; }

        public double MParameter
        {
            get { return _mParameter; }
            set
            {
                if(value<0)
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
                _iterations = value;
                RaisePropertyChanged();
            }
        }

        public int NParameter
        {
            get { return _nParameter; }
            set
            {
                _nParameter = value;
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
                    _multiSystemAgent.RunService(Iterations-_runIteration, MParameter, UpdatingStatus);
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

        public ObservableCollection<LineModel> Contributions { get; set; }

        public RelayCommand AgentsCommand
        {
            get { return new RelayCommand(AgentsBuilder); }
        }

        private void UpdateContributions()
        {
            //need to be optimized

            Contributions = new ObservableCollection<LineModel>();

            foreach (var agent in _multiSystemAgent.Agents)
            {
                Contributions.Add(new LineModel()
                {
                    Name = agent.Name,
                    Contributions = agent.Contributions
                });
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