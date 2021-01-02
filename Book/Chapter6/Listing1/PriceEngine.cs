using System;
using Xunit;

namespace Book.Chapter6.Listing1
{
    public class CustomerControllerTests
    {
        [Fact]
        public void Discount_of_two_products()
        {
            var product1 = new Product("Hand wash");
            var product2 = new Product("Shampoo");
            var sut = new PriceEngine();

            decimal discount = sut.CalculateDiscount(
                product1, product2);

            Assert.Equal(0.02m, discount);
        }
    }

    public class PriceEngine
    {
        public decimal CalculateDiscount(params Product[] product)
        {
            decimal discount = product.Length * 0.01m;
            return Math.Min(discount, 0.2m);
        }
    }

    public class Product
    {
        private string _name;

        public Product(string name)
        {
            _name = name;
        }
    }
}
