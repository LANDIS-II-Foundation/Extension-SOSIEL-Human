using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using MultiAgent;

namespace ApplicationClient.ViewModel
{
    public class AgentsBuilderViewModel : ViewModelBase
    {
        private AgentType _agentType;

        private string _name;

        private Agent<double> _selectedAgent;
        private AgentType _selectedAgentType;

        public AgentsBuilderViewModel()
        {
            MessengerInstance.Register<List<Agent<double>>>(this, e => SetAgents(e));
        }

        public ObservableCollection<Strategy<double>> Strategies { get; set; }

        public ObservableCollection<Agent<double>> Agents { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public AgentType AgentType
        {
            get { return _agentType; }
            set
            {
                _agentType = value;
                RaisePropertyChanged();
            }
        }

        public AgentType SelectedAgentType
        {
            get { return _selectedAgentType; }
            set
            {
                _selectedAgentType = value;
                RaisePropertyChanged();
            }
        }

        public List<Strategy<double>> StrategiesList { get; set; }

        public double Resource { get; set; }

        public RelayCommand DoneCommand
        {
            get { return new RelayCommand(DoneMethod); }
        }

        public Agent<double> SelectedAgent
        {
            get { return _selectedAgent; }
            set
            {
                _selectedAgent = value;
                RaisePropertyChanged();
            }
        }

        private void DoneMethod()
        {
            MessengerInstance.Send(Agents);

            ServiceLocator.Current.GetInstance<INavigationService>().GoBack();
        }

        private void SetAgents(List<Agent<double>> agents)
        {
            Agents = new ObservableCollection<Agent<double>>(agents);
            SelectedAgent = Agents.First();
            SelectedAgentType = SelectedAgent.AgentType;
        }
    }
}