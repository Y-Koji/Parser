using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            string text = File.ReadAllText(@"Json\ApplicationConfig.json");
            Json json = new Json(text);

            dynamic @object = json.Object;

            foreach (var item in @object)
            {
                string key = item.Key;
                object value = item.Value;

                Console.WriteLine($"{key} => {value}");
            }
        }
    }
}
