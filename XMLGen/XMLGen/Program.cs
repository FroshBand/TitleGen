using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XMLGen
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] firstWords = File.ReadAllLines("FirstWords.csv");
            string[] secondWords = File.ReadAllLines("SecondWords.csv");

            List<string> output = new List<string>();
            output.Add("<?xml version=\"1.0\" encoding=\"utf - 8\"?>");
            output.Add("<TitleGen>");

            AddWordList("FirstWords", firstWords, ref output);
            AddWordList("SecondWords", secondWords, ref output);

            output.Add("</TitleGen>");
            File.WriteAllLines("TitleGen.xml", output);
        }

        static void AddWordList(string listTitle, string[] wordList, ref List<string> output)
        {
            output.Add("    <" + listTitle + ">");

            // Skip the first line, which contains column headers
            for (int i = 1; i < wordList.Length; ++i)
            {
                string[] wordData = wordList[i].Split(',');
                output.Add("        <WordData>");
                output.Add("            <Word>" + wordData[0] + "</Word>");
                output.Add("            <Mood>" + wordData[1] + "</Mood>");
                output.Add("            <Syllables>" + wordData[2] + "</Syllables>");
                output.Add("        </WordData>");
            }
            output.Add("    </" + listTitle + ">");
        }
    }
}
