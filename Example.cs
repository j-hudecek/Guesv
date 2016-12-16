using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guesv
{
    class Example
    {
        private void example()
        {
            var g = new Guesv("file.csv");
            Console.WriteLine(g.Header[0]);  //first column name
            Console.WriteLine(g.Data[15][0]);//value of the first column in 15th row
            Console.WriteLine(g[15]["ID"]);  //value of the column "ID" in 15th row
        }
    }
}
