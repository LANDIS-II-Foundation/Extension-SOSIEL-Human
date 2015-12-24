using System.Collections.Generic;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using MultiAgent;

namespace ApplicationClient.ViewModel
{
    public class StrategiesBuilderViewModel : ViewModelBase
    {
        private double _elseCondition;
        private double _ifCondition;
        private string _name;
        private double _thenCondition;

        public StrategiesBuilderViewModel()
        {
            Strategies = new List<Strategy<double>>();
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public List<Strategy<double>> Strategies { get; set; }

        public double IfCondition
        {
            get { return _ifCondition; }
            set
            {
                _ifCondition = value;
                RaisePropertyChanged();
            }
        }

        public double ThenCondition
        {
            get { return _thenCondition; }
            set
            {
                _thenCondition = value;
                RaisePropertyChanged();
            }
        }

        public double ElseCondition
        {
            get { return _elseCondition; }
            set
            {
                _elseCondition = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand AddCommand
        {
            get { return new RelayCommand(AddStrategy); }
        }

        public RelayCommand DoneCommand
        {
            get { return new RelayCommand(DoneMethod); }
        }

        private void DoneMethod()
        {
            MessengerInstance.Send(Strategies);

            ServiceLocator.Current.GetInstance<INavigationService>().GoBack();
        }

        private void AddStrategy()
        {
           
        }
    }
}