namespace ApplicationClient.Model
{
    public class AdjacencyMatrix
    {
        public AdjacencyMatrix(int participantsNumber)
        {
            ParticipantsMatrix = new int[participantsNumber][];
            for (var i = 0; i < participantsNumber; i++)
            {
                ParticipantsMatrix[i] = new int[participantsNumber];
            }

            InitializeIdentityMatrix();
        }

        public int[][] ParticipantsMatrix { get; private set; }

        public void Update(int[][] matrix)
        {
            if (ValidMatrix(matrix))
                ParticipantsMatrix = matrix;
        }

        private bool ValidMatrix(int[][] matrix)
        {
            for (var i = 0; i < matrix.GetLength(0); i++)
                for (var j = 0; j < matrix[i].GetLength(0); j++)
                    if (matrix[i][j] != 0 && matrix[i][j] != 1)
                        return false;
            return true;
        }

        private void InitializeIdentityMatrix()
        {
            for (var i = 0; i < ParticipantsMatrix.GetLength(0); i++)
            {
                for (var j = 0; j < ParticipantsMatrix[i].GetLength(0); j++)
                {
                    ParticipantsMatrix[i][j] = i == j ? 1 : 0;
                }
            }
        }
    }
}