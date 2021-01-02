using System;
using System.Collections.Generic;

namespace Book.Chapter2.Listing1
{
    public class Store
    {
        private readonly Dictionary<Product, int> _inventory = new Dictionary<Product, int>();

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

    public enum Product
    {
        Shampoo,
        Book
    }

    public class Customer
    {
        public bool Purchase(Store store, Product product, int quantity)
        {
            if (!store.HasEnoughInventory(product, quantity))
            {
                return false;
            }

            store.RemoveInventory(product, quantity);

            return true;
        }
    }
}
