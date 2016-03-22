namespace ShoppingCenter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Wintellect.PowerCollections;

    public class ProductCatalog
    {
        private readonly Dictionary<string, OrderedBag<Product>> productsByName =
            new Dictionary<string, OrderedBag<Product>>();

        private readonly Dictionary<string, OrderedBag<Product>> productsByNameAndProducer =
             new Dictionary<string, OrderedBag<Product>>();

        private readonly Dictionary<string, OrderedBag<Product>> productsByProducer =
            new Dictionary<string, OrderedBag<Product>>();

        private readonly OrderedDictionary<double, OrderedBag<Product>> productsByPrice =
            new OrderedDictionary<double, OrderedBag<Product>>();

        public void AddProduct(string name, double price, string producer)
        {
            var product = new Product(name, price, producer);

            // Add to name catalog
            this.productsByName.AddValueToCollection(name, product);

            // Add to producer catalog
            this.productsByProducer.AddValueToCollection(producer, product);

            // Add to name and producer catalog
            var nameProducerCombined = this.CombineNameAndProducer(name, producer);
            this.productsByNameAndProducer.AddValueToCollection(nameProducerCombined, product);

            // Add to pice catalog
            this.productsByPrice.AddValueToCollection(price, product);

            Console.WriteLine("Product added");
        }

        public void DeleteProducts(string producer)
        {
            if (!this.productsByProducer.ContainsKey(producer) ||
                this.productsByProducer[producer].Count == 0)
            {
                Console.WriteLine("No products found");
                return;
            }

            var products = this.productsByProducer[producer];
            var deltedProducts = products.Count;

            // Remove from producer catalog
            this.productsByProducer[producer] = new OrderedBag<Product>();

            // Remove from name catalog
            // Remove from price catalog
            // Remove from name/producer catalog
            foreach (var product in products)
            {
                var nameProducerCombined = this.CombineNameAndProducer(product.Name, product.Producer);

                this.productsByName[product.Name].Remove(product);
                this.productsByNameAndProducer[nameProducerCombined].Remove(product);
                this.productsByPrice[product.Price].Remove(product);
            }

            Console.WriteLine($"{deltedProducts} products deleted");
        }

        public void DeleteProducts(string name, string producer)
        {
            var nameProducerCombined = this.CombineNameAndProducer(name, producer);
            if (!this.productsByNameAndProducer.ContainsKey(nameProducerCombined) ||
                this.productsByNameAndProducer[nameProducerCombined].Count == 0)
            {
                Console.WriteLine("No products found");
                return;
            }

            var products = this.productsByNameAndProducer[nameProducerCombined];
            var deltedProducts = products.Count;

            // Remove from producer catalog
            this.productsByNameAndProducer[nameProducerCombined] = new OrderedBag<Product>();

            // Remove from name catalog
            // Remove from price catalog
            // Remove from name/producer catalog
            foreach (var product in products)
            {
                this.productsByName[product.Name].Remove(product);
                this.productsByProducer[product.Producer].Remove(product);
                this.productsByPrice[product.Price].Remove(product);
            }

            Console.WriteLine($"{deltedProducts} products deleted");
        }

        public void FindProductsByName(string name)
        {
            if (!this.productsByName.ContainsKey(name) ||
                this.productsByName[name].Count == 0)
            {
                Console.WriteLine("No products found");
                return;
            }

            foreach (var product in this.productsByName[name])
            {
                Console.WriteLine(product);
            }
        }

        public void FindProductsByProducer(string producer)
        {
            if (!this.productsByProducer.ContainsKey(producer) ||
                this.productsByProducer[producer].Count == 0)
            {
                Console.WriteLine("No products found");
                return;
            }

            foreach (var product in this.productsByProducer[producer])
            {
                Console.WriteLine(product);
            }
        }

        public void FindProductsByPriceRange(double fromPrice, double toProce)
        {
            var productsInRange = this.productsByPrice.Range(fromPrice, true, toProce, true);

            var allInRange = new OrderedBag<Product>();
            foreach (var productList in productsInRange.Values)
            {
                foreach (var product in productList)
                {
                    allInRange.Add(product);
                }
            }

            if (allInRange.Count == 0)
            {
                Console.WriteLine("No products found");
                return;
            }

            foreach (var product in allInRange)
            {
                Console.WriteLine(product);
            }
        }

        private string CombineNameAndProducer(string name, string producer)
        {
            var result = name + ">*<" + producer;

            return result;
        }
    }

    public class Product : IComparable<Product>
    {
        public Product(string name, double price, string producer)
        {
            this.Name = name;
            this.Price = price;
            this.Producer = producer;
        }

        public string Name { get; }

        public double Price { get; }

        public string Producer { get; }

        public int CompareTo(Product other)
        {
            var compare = this.Name.CompareTo(other.Name);
            if (compare == 0)
            {
                compare = this.Producer.CompareTo(other.Producer);
                if (compare == 0)
                {
                    compare = this.Price.CompareTo(other.Price);
                }
            }

            return compare;
        }

        public override string ToString()
        {
            return string.Format(
                "{3}{0};{1};{2:F2}{4}",
                this.Name,
                this.Producer,
                this.Price,
                "{",
                "}");
        }
    }

    public static class DictExtensions
    {
        public static void AddValueToCollection<TKey, TValue, TCollection>(
            this IDictionary<TKey, TCollection> dict, TKey key, TValue value)
                where TCollection : ICollection<TValue>, new()
        {
            TCollection collection;
            if (!dict.TryGetValue(key, out collection))
            {
                dict.Add(key, new TCollection());
            }

            dict[key].Add(value);
        }
    }

    public static class ShoppingCenter
    {
        private static ProductCatalog productCatalog;

        public static void Main()
        {
            productCatalog = new ProductCatalog();

            var numberOfCommands = int.Parse(Console.ReadLine());
            for (int i = 0; i < numberOfCommands; i++)
            {
                var command = Console.ReadLine();

                var separatorIndex = command.IndexOf(' ');
                var commandName = command.Substring(0, separatorIndex);
                var commandArgs = command
                    .Substring(separatorIndex + 1, command.Length - separatorIndex - 1)
                    .Split(';')
                    .ToArray();

                ProcessCommands(commandName, commandArgs);
            }
        }

        private static void ProcessCommands(string commandName, string[] commandArgs)
        {
            switch (commandName)
            {
                case "AddProduct":
                    productCatalog.AddProduct(
                        commandArgs[0],
                        double.Parse(commandArgs[1]),
                        commandArgs[2]);
                    break;
                case "DeleteProducts":
                    if (commandArgs.Length == 1)
                    {
                        productCatalog.DeleteProducts(commandArgs[0]);
                    }
                    else
                    {
                        productCatalog.DeleteProducts(commandArgs[0], commandArgs[1]);
                    }
                    break;
                case "FindProductsByName":
                    productCatalog.FindProductsByName(commandArgs[0]);
                    break;
                case "FindProductsByProducer":
                    productCatalog.FindProductsByProducer(commandArgs[0]);
                    break;
                case "FindProductsByPriceRange":
                    productCatalog.FindProductsByPriceRange(
                        double.Parse(commandArgs[0]),
                        double.Parse(commandArgs[1]));
                    break;
                default:
                    throw new NotImplementedException("Command not implemented");
            }
        }
    }
}
