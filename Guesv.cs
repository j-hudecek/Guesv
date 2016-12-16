using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Guesv
{
    public class Guesv
    {
        public readonly string[] Header;
        public readonly string[][] Data;

        public Dictionary<string, string> this[int index]
        {
            get
            {
                var ret = new Dictionary<string, string>();
                for (int i = 0; i < data[index].Count; i++)
                {
                    ret.Add(header[i], data[index][i]);
                }
                return ret;
            }
        }

        public Guesv(string filename)
        {
            this.filename = filename;
            LoadFile(filename, true);

            foreach (var kv in header2i)
            {
                var values = data.Select(x => x[kv.Value]);
                if (values.All(x => !String.IsNullOrEmpty(x)))
                { //all rows have a value in this column
                    allFilledHeaders.Add(kv.Key);
                    if (values.Distinct().Count() == data.Count())
                        //each value is unique
                        allFilledAndUniqueHeaders.Add(kv.Key);
                }
                examples.Add(kv.Key, values.FirstOrDefault(x => !String.IsNullOrEmpty(x)));
            }
            Header = header.ToArray();
            Data = data.Select(x=>x.ToArray()).ToArray();
        }

        private List<string> header;
        private Dictionary<string, int> header2i;
        private int RowCount;

        private string filename;

        private const int SAMPLE_LINES = 5;
        private const int BULK_INSERT_ROWS = 10000;


        private List<List<string>> data;
        private char realsep;
        private List<string> allFilledHeaders = new List<string>();
        private List<string> allFilledAndUniqueHeaders = new List<string>();
        private Dictionary<string, string> examples = new Dictionary<string, string>();
        private bool doublequoted = false;

        private void LoadFile(string filename, bool onlySample = false)
        {
            string wholefile = File.ReadAllText(filename);
            GuessSeparator(wholefile);
            int i = 0;
            header = GetLine(wholefile, ref i);
            if (header.Count == 1)
            { //only one column? Filemaker pro csv files can have a line like
              //"0,""pGEX-6p-1-hUSP7CD-HUBL C223A WE285DA"",""hUSP7"",""pGEX-6p-1"""
                doublequoted = true;
                i = 0;
                header = GetLine(wholefile, ref i);
            }
            InitHeader();
            data = GetData(wholefile, i, onlySample);
            RowCount = data.Count;
        }

        private List<string> GetRow(int row)
        {
            return data[row];
        }

        private void EnsureWholeFileLoaded()
        {
            LoadFile(filename);
        }

        private List<List<string>> GetData(string wholefile, int i, bool onlySample = false)
        {
            var data = new List<List<string>>();
            while (true)
            {
                var line = GetLine(wholefile, ref i);
                if (line.Count == 0)
                    break; //EOF
                data.Add(line);
                if (onlySample && data.Count > SAMPLE_LINES)
                    break;
            }

            return data;
        }

        private List<string> GetLine(string wholefile, ref int i)
        {
            var line = new List<string>();
            bool quoted = false;
            int start = i;
            for (; true; i++)
            {
                if (wholefile.Length <= i)
                    break;
                if (wholefile[i] == '"')
                {
                    if (doublequoted)
                    {
                        if (i < wholefile.Length - 1 && wholefile[i + 1] == '"')
                        {
                            quoted = !quoted;
                            i++; //skip the second quote as well
                        }
                    }
                    else
                        quoted = !quoted;
                    continue;
                }
                if (wholefile[i] == '\r')
                    continue;
                if (!quoted && wholefile[i] == '\n')
                    break;
                if (!quoted && wholefile[i] == realsep)
                {
                    line.Add(wholefile.Substring(start, i - start).Replace("\n", " ").Replace("\"", "").Replace("\r", ""));
                    start = i + 1;
                    continue;
                }
            }
            if (i > start)
                line.Add(wholefile.Substring(start, i - start - 1).Replace("\n", " ").Replace("\"", "").Replace("\r", ""));
            i++;
            return line;
        }

        private void InitHeader()
        {
            header2i = new Dictionary<string, int>();
            for (int i = 0; i < header.Count; i++)
            {
                header2i.Add(header[i], i);
            }
        }

        private void GuessSeparator(string wholefile)
        {
            var firstline = wholefile.Substring(0, wholefile.IndexOf("\n"));
            var secondline = wholefile.Substring(firstline.Length, wholefile.IndexOf("\n", firstline.Length));
            var seps = new char[] { '\t', ',', ';', ':' };
            realsep = seps[0];
            int mindiff = Int32.MaxValue;
            foreach (var sep in seps)
            {
                var sepsin1stline = firstline.Count(x => x == sep);
                var sepsin2ndline = secondline.Count(x => x == sep);
                if (sepsin1stline < 2 || sepsin2ndline < 2)
                    continue;
                if (Math.Abs(sepsin1stline - sepsin2ndline) < mindiff)
                {
                    realsep = sep;
                    mindiff = Math.Abs(sepsin1stline - sepsin2ndline);
                }
            }
        }
    }
}
