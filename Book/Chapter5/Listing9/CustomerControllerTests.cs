using System;
using System.Collections.Generic;
using Moq;
using Xunit;

namespace Book.Chapter5.Listing9
{
    public class CustomerControllerTests
    {
        [Fact(Skip = "Concept illustration only")]
        public void Successful_purchase()
        {
            var mock = new Mock<IEmailGateway>();
            var sut = new CustomerController(mock.Object);

            bool isSuccess = sut.Purchase(
                customerId: 1, productId: 2, quantity: 5);

            Assert.True(isSuccess);
            mock.Verify(
                x => x.SendReceipt(
                    "customer@email.com", "Shampoo", 5),
                Times.Once);
        }
    }

    public class CustomerTests
    {
        [Fact]
        public void Purchase_succeeds_when_enough_inventory()
        {
            var storeMock = new Mock<IStore>();
            storeMock
                .Setup(x => x.HasEnoughInventory(Product.Shampoo, 5))
                .Returns(true);
            var customer = new Customer();

            bool success = customer.Purchase(storeMock.Object, Product.Shampoo, 5);

            Assert.True(success);
            storeMock.Verify(
                x => x.RemoveInventory(Product.Shampoo, 5),
                Times.Once);
        }
    }

    public class CustomerController
    {
        private readonly CustomerRepository _customerRepository;
        private readonly ProductRepository _productRepository;
        private readonly Store _mainStore;
        private readonly IEmailGateway _emailGateway;

        public CustomerController(IEmailGateway emailGateway)
        {
            _emailGateway = emailGateway;
        }

        public bool Purchase(int customerId, int productId, int quantity)
        {
            Customer customer = _customerRepository.GetById(customerId);
            Product product = _productRepository.GetById(productId);

            bool isSuccess = customer.Purchase(_mainStore, product, quantity);

            if (isSuccess)
            {
                _emailGateway.SendReceipt(customer.Email, product.Name, quantity);
            }

            return isSuccess;
        }
    }

    public class EmailGateway : IEmailGateway
    {
        public void SendReceipt(string email, string productName, int quantity)
        {
        }
    }

    public interface IEmailGateway
    {
        void SendReceipt(string email, string productName, int quantity);
    }

    internal class ProductRepository
    {
        public Product GetById(int productId)
        {
            return new Product();
        }
    }

    internal class CustomerRepository
    {
        public Customer GetById(int customerId)
        {
            return new Customer();
        }
    }

    public interface IStore
    {
        bool HasEnoughInventory(Product product, int quantity);
        void RemoveInventory(Product product, int quantity);
        void AddInventory(Product product, int quantity);
        int GetInventory(Product product);
    }

    public class Store : IStore
    {
        private readonly Dictionary<Product, int> _inventory = new Dictionary<Product, int>();
        public int Id { get; set; }

        public bool HasEnoughInventory(Product product, int quantity)
        {
            return GetInventory(product) >= quantity;
        }

        public void RemoveInventory(Product product, int quantity)
        {
            if (!HasEnoughInventory(product, quantity))
            {
                throw new Exception("Not enough inventory");
            }

            _inventory[product] -= quantity;
        }

        public void AddInventory(Product product, int quantity)
        {
            if (_inventory.ContainsKey(product))
            {
                _inventory[product] += quantity;
            }
            else
            {
                _inventory.Add(product, quantity);
            }
        }

        public int GetInventory(Product product)
        {
            bool productExists = _inventory.TryGetValue(product, out int remaining);
            return productExists ? remaining : 0;
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public static Product Shampoo { get; set; }
    }

    public class Customer
    {
        public bool Purchase(IStore store, Product product, int quantity)
        {
            if (!store.HasEnoughInventory(product, quantity))
            {
                return false;
            }

            store.RemoveInventory(product, quantity);

            return true;
        }

        public string Email { get; set; }
    }
}
