using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Namotion.Reflection.Demo
{
    public class MySampleClass
    {
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the optional middle name.
        /// </summary>
        //public string? MiddleName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="middleName">The optional middle name.</param>
        /// <param name="counter">The counter.</param>
        public void Update(string firstName, /*string? middleName,*/ Dictionary<int, int?> counter)
        {

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var properties = typeof(MySampleClass).GetContextualProperties();
            var parameters = typeof(MySampleClass)
                .GetMethod(nameof(MySampleClass.Update))
                .GetContextualParameters();

            Console.WriteLine(nameof(MySampleClass));

            Console.WriteLine("# Properties");
            foreach (var property in properties)
            {
                Console.WriteLine(property);
                Console.WriteLine("XML Description: " + property.PropertyInfo.GetXmlDocsSummary());
            }

            Console.WriteLine("# Parameters: " + nameof(MySampleClass.Update));
            foreach (var parameter in parameters)
            {
                Console.WriteLine(parameter);
                Console.WriteLine("XML Description: " + parameter.ParameterInfo.GetXmlDocs());
            }
        }
    }
}
