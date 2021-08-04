using System;
using System.Collections.Generic;
using System.Linq;

namespace Namotion.Reflection.Demo
{
    public class Person
    {
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; } = default!;

        /// <summary>
        /// Gets or sets the optional middle name.
        /// </summary>
        public string? MiddleName { get; set; }

        /// <summary>
        /// Updates the person.
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="middleName">The optional middle name.</param>
        /// <param name="counter">The counter.</param>
        public void Update(string firstName, string? middleName, Dictionary<int, int?> counter)
        {

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var properties = typeof(Person)
                .GetContextualProperties();

            var parameters = typeof(Person)
                .GetMethod(nameof(Person.Update))!
                .GetParameters()
                .Select(p => p.ToContextualParameter());

            Console.WriteLine(nameof(Person));

            Console.WriteLine("# Person Properties: ");
            foreach (var property in properties)
            {
                Console.WriteLine("  " + property);
                Console.WriteLine("    XML Description: " + property.PropertyInfo.GetXmlDocsSummary());
            }

            Console.WriteLine("# Update Method Parameters: ");
            foreach (var parameter in parameters)
            {
                Console.WriteLine("  " + parameter);
                Console.WriteLine("    XML Description: " + parameter.ParameterInfo.GetXmlDocs());
            }

            Console.WriteLine("Press <any> key to quit...");
            Console.ReadKey();
        }
    }
}
