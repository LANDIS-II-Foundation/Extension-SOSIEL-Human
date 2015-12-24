using System.Collections.ObjectModel;
using System.Linq;
using ApplicationClient.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;

namespace ApplicationClient.ViewModel
{
    public class CommunicationViewModel : ViewModelBase
    {
        private AdjacencyMatrix _adjacencyMatrix;

        [PreferredConstructor]
        public CommunicationViewModel()
        {
            MessengerInstance.Register<int>(this, e => update(e));

            MatrixCollection = new ObservableCollection<MapRule>();
        }

        public CommunicationViewModel(int participantsNumber)
        {
            if (participantsNumber < 0)
                _adjacencyMatrix = new AdjacencyMatrix(0);
            else
            {
                _adjacencyMatrix = new AdjacencyMatrix(participantsNumber);
            }

            MatrixCollection = new ObservableCollection<MapRule>();
        }

        public ObservableCollection<MapRule> MatrixCollection { get; set; }

        public RelayCommand BuildCommunicationMapCommand
        {
            get { return new RelayCommand(CreateCommunicationMap); }
        }

        private void update(int participantsNumber)
        {
            if (participantsNumber < 0)
                _adjacencyMatrix = new AdjacencyMatrix(0);
            else
            {
                _adjacencyMatrix = new AdjacencyMatrix(participantsNumber);
            }

            MatrixCollection = new ObservableCollection<MapRule>();

            for (var i = 0; i < _adjacencyMatrix.ParticipantsMatrix.GetLength(0); i++)
            {
                for (var j = 0; j < _adjacencyMatrix.ParticipantsMatrix[0].GetLength(0); j++)
                {
                    MatrixCollection.Add(new MapRule
                    {
                        Column = i,
                        Raw = j,
                        Selected = _adjacencyMatrix.ParticipantsMatrix[i][j] != 0
                    });
                }
            }
        }

        private void CreateCommunicationMap()
        {
            _adjacencyMatrix.Update(GetMatrix(MatrixCollection));

            MessengerInstance.Send(_adjacencyMatrix.ParticipantsMatrix);
            ServiceLocator.Current.GetInstance<INavigationService>().GoBack();
        }

        private int[][] GetMatrix(ObservableCollection<MapRule> matrixCollection)
        {
            var result = new int[_adjacencyMatrix.ParticipantsMatrix.GetLength(0)][];

            for (var i = 0; i < result.Length; i++)
                result[i] = new int[_adjacencyMatrix.ParticipantsMatrix[i].GetLength(0)];

            for (var i = 0; i < result.GetLength(0); i++)
            {
                for (var j = 0; j < result[i].GetLength(0); j++)
                {
                    result[i][j] = matrixCollection.Where(x => x.Column == j && x.Raw == i).First().Selected ? 1 : 0;
                }
            }
            return result;
        }
    }
}