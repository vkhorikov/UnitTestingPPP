using Book.Chapter2.Listing1;
using Xunit;

namespace Book.Chapter3.CustomerTests_3
{
    public class CustomerTests
    {
        private readonly Store _store;
        private readonly Customer _sut;

        public CustomerTests()
        {
            _store = new Store();
            _store.AddInventory(Product.Shampoo, 10);
            _sut = new Customer();
        }

        [Fact]
        public void Purchase_succeeds_when_enough_inventory()
        {
            bool success = _sut.Purchase(_store, Product.Shampoo, 5);

            Assert.True(success);
            Assert.Equal(5, _store.GetInventory(Product.Shampoo));
        }

        [Fact]
        public void Purchase_fails_when_not_enough_inventory()
        {
            bool success = _sut.Purchase(_store, Product.Shampoo, 15);

            Assert.False(success);
            Assert.Equal(10, _store.GetInventory(Product.Shampoo));
        }
    }
}
