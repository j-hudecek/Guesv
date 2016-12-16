# Guesv
C# library for loading csv, tsv or other regularly shaped files guess-separated-values.

# Usage
    var g = new Guesv("file.csv");
Loads the file. It searches a couple of first lines for characters which appear similar number of times outside quotes and uses that as a separator.    
It can also handle quoted and double quoted (hello FileMaker) CSV/TSV/semicolon-SV/... files.

Accessing values can be done like this:

    Console.WriteLine(g.Header[0]);  //first column name
    Console.WriteLine(g.Data[15][0]);//value of the first column in 15th row
    Console.WriteLine(g[15]["ID"]);  //value of the column "ID" in 15th row

