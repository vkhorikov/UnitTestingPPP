using Moq;
using Xunit;

namespace Book.Chapter2.Listing2
{
    public class CustomerTests
    {
        [Fact]
        public void Purchase_succeeds_when_enough_inventory()
        {
            // Arrange
            var storeMock = new Mock<IStore>();
            storeMock
                .Setup(x => x.HasEnoughInventory(Product.Shampoo, 5))
                .Returns(true);
            var customer = new Customer();

            // Act
            bool success = customer.Purchase(storeMock.Object, Product.Shampoo, 5);

            // Assert
            Assert.True(success);
            storeMock.Verify(x => x.RemoveInventory(Product.Shampoo, 5), Times.Once);
        }

        [Fact]
        public void Purchase_fails_when_not_enough_inventory()
        {
            // Arrange
            var storeMock = new Mock<IStore>();
            storeMock
                .Setup(x => x.HasEnoughInventory(Product.Shampoo, 5))
                .Returns(false);
            var customer = new Customer();

            // Act
            bool success = customer.Purchase(storeMock.Object, Product.Shampoo, 5);

            // Assert
            Assert.False(success);
            storeMock.Verify(x => x.RemoveInventory(Product.Shampoo, 5), Times.Never);
        }
    }
}
