using System;
using System.Collections.Generic;

namespace Book.Chapter2.Listing2
{
    public class Store : IStore
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

    public interface IStore
    {
        bool HasEnoughInventory(Product product, int quantity);
        void RemoveInventory(Product product, int quantity);
        void AddInventory(Product product, int quantity);
        int GetInventory(Product product);
    }

    public enum Product
    {
        Shampoo,
        Book
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
    }
}
