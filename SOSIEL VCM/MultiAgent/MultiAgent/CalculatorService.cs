using System.Linq;

namespace MultiAgent
{
    public class CalculatorService
    {
        public double CalculateNewPayoff(double contribution, double endowment, double[] contributions,
            double mParameter)
        {
            return (contribution/endowment)*(endowment - contribution) +
                   (mParameter/contributions.Length)*contributions.Sum();
        }
    }
}