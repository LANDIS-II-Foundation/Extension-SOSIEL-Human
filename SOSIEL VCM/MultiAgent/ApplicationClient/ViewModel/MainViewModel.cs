using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
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
        private readonly MultiAgentSystem _multiSystemAgent;
        private readonly FrameNavigationService _navigator;
        private int _rParameter;
        private double _mParameter;
        private int _nParameter;
        private int _runIteration;
        private double _endowment;

        public MainViewModel()
        {
            _navigator = (FrameNavigationService) ServiceLocator.Current.GetInstance<INavigationService>();

            Updates = new ObservableCollection<string>();

            Func<double, double> cooperationFunc = averageCotribution => averageCotribution >= 5 ? 10 : 0;
            Func<double, double> trendFunc = averageCotribution => averageCotribution;
            Func<double, double> freeRiderFunc = averageCotribution => 0;

            _multiSystemAgent = new MultiAgentSystem(cooperationFunc, trendFunc, freeRiderFunc);
            EndowmentParameter = 10;
            NParameter = 1;
            RParameter = 1;

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

        public double EndowmentParameter
        {
            get { return _endowment; }
            set
            {
                if (value < 0)
                    return;
                _endowment = value;
                RaisePropertyChanged();
            }
        }

        public int RParameter
        {
            get { return _rParameter; }
            set
            {
                _rParameter = value < 1 ? 1 : value;
                RaisePropertyChanged();
            }
        }

        public int NParameter
        {
            get { return _nParameter; }
            set
            {
                _nParameter = value < 1 ? 1 : value;
                _multiSystemAgent.InititalizeAgents(value, EndowmentParameter);
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
                    _multiSystemAgent.RunService(RParameter - _runIteration, MParameter);
                    _runIteration = RParameter;
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
                    if (++_runIteration > RParameter)
                        return;
                    _multiSystemAgent.RunServiceOnce(++_runIteration, MParameter);
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

        public event UpdateD3 UpdateD3;

        public ObservableCollection<LineModel> Contributions { get; set; }

        public RelayCommand AgentsCommand
        {
            get { return new RelayCommand(AgentsBuilder); }
        }

        private void UpdateContributions()
        {
            Contributions = new ObservableCollection<LineModel>();

            foreach (var agent in _multiSystemAgent.Agents)
            {
                Contributions.Add(new LineModel(){Name = agent.Name, Data = agent.Contributions});
            }

            if (UpdateD3 != null) 
                UpdateD3(this, null);
            RaisePropertyChanged("Contributions");
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

    public struct LineModel
    {
        public string Name { get; set; }
        public List<double> Data { get; set; }
    }

    public delegate void UpdateD3(object sender, EventArgs args);
}