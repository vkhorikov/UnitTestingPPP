using System;
using Book.Chapter2.Listing1;
using Xunit;

namespace Book.Chapter3.CustomerTests_4
{
    public class CustomerTests
    {
        [Fact]
        public void Purchase_succeeds_when_enough_inventory()
        {
            Store store = CreateStoreWithInventory(Product.Shampoo, 10);
            Customer sut = CreateCustomer();

            bool success = sut.Purchase(store, Product.Shampoo, 5);

            Assert.True(success);
            Assert.Equal(5, store.GetInventory(Product.Shampoo));
        }

        [Fact]
        public void Purchase_fails_when_not_enough_inventory()
        {
            Store store = CreateStoreWithInventory(Product.Shampoo, 10);
            Customer sut = CreateCustomer();

            bool success = sut.Purchase(store, Product.Shampoo, 15);

            Assert.False(success);
            Assert.Equal(10, store.GetInventory(Product.Shampoo));
        }

        private Store CreateStoreWithInventory(Product product, int quantity)
        {
            Store store = new Store();
            store.AddInventory(product, quantity);
            return store;
        }

        private static Customer CreateCustomer()
        {
            return new Customer();
        }
    }

    public class CustomerTests2 : IntegrationTests
    {
        [Fact]
        public void Purchase_succeeds_when_enough_inventory()
        {
            /* ... */
        }
    }

    public abstract class IntegrationTests : IDisposable
    {
        protected readonly Database _database;

        protected IntegrationTests()
        {
            _database = new Database();
        }

        public void Dispose()
        {
            _database.Dispose();
        }
    }

    public class Database
    {
        public void Dispose()
        {
        }
    }
}
