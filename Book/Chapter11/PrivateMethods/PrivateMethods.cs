using System;
using System.Collections.Generic;

namespace Book.Chapter11.PrivateMethods
{
    public class Order
    {
        private Customer _customer;
        private List<Product> _products;

        public string GenerateDescription()
        {
            return $"Customer name: {_customer.Name}, " +
                $"total number of products: {_products.Count}, " +
                $"total price: {GetPrice()}";
        }

        private decimal GetPrice()
        {
            decimal basePrice = /* Calculate based on _products */ 0;
            decimal discounts = /* Calculate based on _customer */ 0;
            decimal taxes = /* Calculate based on _products */ 0;
            return basePrice - discounts + taxes;
        }
    }

    public class Product
    {
    }

    public class Customer
    {
        public object Name { get; set; }
    }

    public class OrderV2
    {
        private Customer _customer;
        private List<Product> _products;

        public string GenerateDescription()
        {
            var calculator = new PriceCalculator();

            return $"Customer name: {_customer.Name}, " +
                $"total number of products: {_products.Count}, " +
                $"total price: {calculator.Calculate(_customer, _products)}";
        }
    }

    public class PriceCalculator
    {
        public decimal Calculate(Customer customer, List<Product> products)
        {
            decimal basePrice = /* Calculate based on products */ 0;
            decimal discounts = /* Calculate based on customer */ 0;
            decimal taxes = /* Calculate based on products */ 0;
            return basePrice - discounts + taxes;
        }
    }

    public class Inquiry
    {
        public bool IsApproved { get; private set; }
        public DateTime? TimeApproved { get; private set; }

        private Inquiry(bool isApproved, DateTime? timeApproved)
        {
            if (isApproved && !timeApproved.HasValue)
                throw new Exception();

            IsApproved = isApproved;
            TimeApproved = timeApproved;
        }

        public void Approve(DateTime now)
        {
            if (IsApproved)
                return;

            IsApproved = true;
            TimeApproved = now;
        }
    }
}
