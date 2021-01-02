using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Book.Chapter11.MockingClasses
{
    public class StatisticsCalculator
    {
        public (double totalWeight, double totalCost) Calculate(
            int customerId)
        {
            List<DeliveryRecord> records = GetDeliveries(customerId);

            double totalWeight = records.Sum(x => x.Weight);
            double totalCost = records.Sum(x => x.Cost);

            return (totalWeight, totalCost);
        }

        public virtual List<DeliveryRecord> GetDeliveries(int customerId)
        {
            /* Call an out-of-process dependency
            to get the list of deliveries */
            return new List<DeliveryRecord>();
        }
    }

    public class DeliveryRecord
    {
        public double Weight { get; set; }
        public double Cost { get; set; }
    }

    public class CustomerController
    {
        private readonly StatisticsCalculator _calculator;

        public CustomerController(StatisticsCalculator calculator)
        {
            _calculator = calculator;
        }

        public string GetStatistics(int customerId)
        {
            (double totalWeight, double totalCost) = _calculator
                .Calculate(customerId);

            return 
                $"Total weight delivered: {totalWeight}. " +
                $"Total cost: {totalCost}";
        }
    }

    public class Tests
    {
        [Fact]
        public void Customer_with_no_deliveries()
        {
            // Arrange
            var stub = new Mock<StatisticsCalculator> { CallBase = true };
            stub.Setup(x => x.GetDeliveries(1))
                .Returns(new List<DeliveryRecord>());
            var sut = new CustomerController(stub.Object);

            // Act
            string result = sut.GetStatistics(1);

            // Assert
            Assert.Equal("Total weight delivered: 0. Total cost: 0", result);
        }
    }

    public class DeliveryGateway : IDeliveryGateway
    {
        public List<DeliveryRecord> GetDeliveries(int customerId)
        {
            /* Call an out-of-process dependency
            to get the list of deliveries */
            return new List<DeliveryRecord>();
        }
    }

    public interface IDeliveryGateway
    {
        List<DeliveryRecord> GetDeliveries(int customerId);
    }

    public class StatisticsCalculator2
    {
        public (double totalWeight, double totalCost) Calculate(
            List<DeliveryRecord> records)
        {
            double totalWeight = records.Sum(x => x.Weight);
            double totalCost = records.Sum(x => x.Cost);

            return (totalWeight, totalCost);
        }
    }

    public class CustomerController2
    {
        private readonly StatisticsCalculator2 _calculator;
        private readonly IDeliveryGateway _gateway;

        public CustomerController2(StatisticsCalculator2 calculator, IDeliveryGateway gateway)
        {
            _calculator = calculator;
            _gateway = gateway;
        }

        public string GetStatistics(int customerId)
        {
            List<DeliveryRecord> records = _gateway.GetDeliveries(customerId);
            (double totalWeight, double totalCost) = _calculator.Calculate(records);

            return $"Total weight delivered: {totalWeight}. Total cost: {totalCost}";
        }
    }
}
