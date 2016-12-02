using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SaveDictionary
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<Tuple<int, int, int, int, int, string>, double> Q = new Dictionary<Tuple<int,int,int,int,int,string>,double>();
            Dictionary<Tuple<int, int, int, int, int, string>, int> N = new Dictionary<Tuple<int, int, int, int, int, string>, int>();

            var tuple = Tuple.Create(11,1,0,1,3,"UP");
            Q.Add(tuple, .77);
            N.Add(tuple, 1);

            Write(Q, @"C:\Users\mabiscoc\Documents\Visual Studio 2013\Projects\SaveDictionary\SaveDictionary\Q.txt");
            Write(N, @"C:\Users\mabiscoc\Documents\Visual Studio 2013\Projects\SaveDictionary\SaveDictionary\N.txt");

            Dictionary<Tuple<int, int, int, int, int, string>, double> newQ = ReadQ(@"C:\Users\mabiscoc\Documents\Visual Studio 2013\Projects\SaveDictionary\SaveDictionary\Q.txt");
            Dictionary<Tuple<int, int, int, int, int, string>, int> newN = ReadN(@"C:\Users\mabiscoc\Documents\Visual Studio 2013\Projects\SaveDictionary\SaveDictionary\N.txt");
            

        }

        static void Write(Dictionary<Tuple<int, int, int, int, int, string>, double> dictionary, string file)
        {
            using (StreamWriter f = new StreamWriter(file))
                foreach (var entry in dictionary)
                {
                    f.WriteLine("{0} {1}", entry.Key, entry.Value);
                }
        }

        static void Write(Dictionary<Tuple<int, int, int, int, int, string>, int> dictionary, string file)
        {
            using (StreamWriter f = new StreamWriter(file))
                foreach (var entry in dictionary)
                {
                    f.WriteLine("{0} {1}", entry.Key, entry.Value);
                }
        }

        static Dictionary<Tuple<int, int, int, int, int, string>, double> ReadQ(string file)
        {
            Dictionary<Tuple<int, int, int, int, int, string>, double> d = new Dictionary<Tuple<int,int,int,int,int,string>,double>();
            using (var sr = new StreamReader(file))
            {
                string line = null;

                // while it reads a key
                while ((line = sr.ReadLine()) != null)
                {
                    string tuple = line.Split(')')[0].Trim('(');
                    double value = Double.Parse(line.Split(')')[1]);

                    Tuple<int, int, int, int, int, string> newTuple = Tuple.Create(Int32.Parse(tuple.Split(',')[0]),Int32.Parse(tuple.Split(',')[1]),Int32.Parse(tuple.Split(',')[2]),Int32.Parse(tuple.Split(',')[3]),Int32.Parse(tuple.Split(',')[4]),tuple.Split(',')[5].ToString());
                    d.Add(newTuple, value);
                    //d.Add(line, sr.ReadLine());
                }
            }
            return d;
        }

        static Dictionary<Tuple<int, int, int, int, int, string>, int> ReadN(string file)
        {
            Dictionary<Tuple<int, int, int, int, int, string>, int> d = new Dictionary<Tuple<int, int, int, int, int, string>, int>();
            using (var sr = new StreamReader(file))
            {
                string line = null;

                // while it reads a key
                while ((line = sr.ReadLine()) != null)
                {
                    string tuple = line.Split(')')[0].Trim('(');
                    int value = Int32.Parse(line.Split(')')[1]);

                    Tuple<int, int, int, int, int, string> newTuple = Tuple.Create(Int32.Parse(tuple.Split(',')[0]), Int32.Parse(tuple.Split(',')[1]), Int32.Parse(tuple.Split(',')[2]), Int32.Parse(tuple.Split(',')[3]), Int32.Parse(tuple.Split(',')[4]), tuple.Split(',')[5].ToString());
                    d.Add(newTuple, value);
                    //d.Add(line, sr.ReadLine());
                }
            }
            return d;
        }
    }

}
