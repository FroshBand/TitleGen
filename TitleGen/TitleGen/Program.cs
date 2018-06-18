// Matthew Calligaro
// June 2018
// matthewcalligaro@hotmail.com

namespace TitleGen
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;

    [XmlRoot("TitleGen")]
    class Program
    {
        // Constants
        const int Tolerance = 1;
        const int DefaultNumTitles = 10;
        const int MaxSyllables = 5;
        const int MaxMood = 5;
        const int MaxIters = 1000000;
        static readonly char[] vowels = { 'A', 'E', 'I', 'O', 'U', 'a', 'e', 'i', 'o', 'u' };
        static string DefaultPrompt = "Please enter a mood from -" + MaxMood + " (most negative) to " + MaxMood + " (most positive)";
        static string InvalidInputText = "Invalid input, enter \"h\" for help";
        static string[] HelpText = {
            "TitleGen - a procedural song title generator",
            ">>Vocabulary",
            "mood: a rating for the feeling of the title, ranging from -" + MaxMood + " (heaviest) to " + MaxMood + " (happiest)",
            "",
            ">>Inputs",
            "<integer>: produces titles with a mood within " + Tolerance + "of the given integer value",
            "syl: allows titles of any number of syllables",
            "syl <integer>: restricts titles to a particular number of syllables",
            "num <integer>: sets the number of titles to print per entry",
            "ver: toggles the verbose flag, which prints the mood and syllables of each generated title",
            "help: prints this help dialog",
            "quit: closes TitleGen"};

        // member variables 
        static Random rand = new Random();
        static TitleGenerator titleGenerator;
        static string prompt = DefaultPrompt;
        static int? syllables = null;
        static int numTitles = DefaultNumTitles;
        static bool verbose = false;
        
        /// <summary>
        /// The number of syllables to which titles are restricted
        /// null for unrestricted
        /// </summary>
        static int? Syllables
        {
            get
            {
                return syllables;
            }
            set
            {         
                prompt = DefaultPrompt;
                syllables = value;
                if (syllables != null)
                {
                    syllables = syllables > MaxSyllables ? MaxSyllables : syllables;
                    if (syllables == MaxSyllables)
                    {
                        prompt += " [" + syllables.ToString() + " or more syllables]";
                    }
                    else
                    {
                        prompt += " [" + syllables.ToString() + " syllables]";
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            InitializeTitleGenerator();

            bool exit = false;
            bool printPrompt = true;
            while (!exit)
            {
                if (printPrompt)
                {
                    Console.WriteLine(prompt);
                }
                printPrompt = HandleInput(Console.ReadLine(), out exit);
                Console.WriteLine();
            }

            Console.WriteLine("Goodbye!");
        }

        /// <summary>
        /// Initializes titleGenerator by deserializing TitleGen.xml
        /// </summary>
        private static void InitializeTitleGenerator()
        {
            try
            {
                XmlSerializer dsr = new XmlSerializer(typeof(TitleGenerator));
                using (StreamReader stream = new StreamReader("TitleGen.xml"))
                {
                    titleGenerator = (TitleGenerator)dsr.Deserialize(stream);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Could not find TitleGen.xml.  Please ensure that TitleGen.xml is in the same directory as TitleGen.exe and relaunch the program.");
                Console.ReadLine();

                // Exit with an error code of 1
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Processes user input
        /// </summary>
        /// <param name="input">The line the user entered</param>
        /// <param name="exit">set to true if the program should exit</param>
        /// <returns>true if the program should quit</returns>
        private static bool HandleInput(string input, out bool exit)
        {
            exit = false;
            int parsedInt;

            // First, assume the input is a mood value parsable as an int
            if (int.TryParse(input, out parsedInt))
            {
                Generate(parsedInt);
                return true;
            }

            // Otherwise, treat the input as a text input
            string[] parsedInput = input.ToLower().Split(' ');
            switch(parsedInput[0])
            {
                case "q":
                case "quit":
                case "exit:":
                case "close":
                    exit = true;
                    return false;

                case "":
                    return false;

                case "h":
                case "help":
                    Help();
                    break;

                case "s":
                case "syl":
                case "syllables":
                    if (parsedInput.Length >= 2 && int.TryParse(parsedInput[1], out parsedInt))
                    {
                        Syllables = parsedInt;
                        Console.WriteLine("Will now restrict titles to " + Syllables + " syllables");
                    }
                    else
                    {
                        Console.WriteLine("Title syllables unrestricted");
                        syllables = null;
                    }
                    break;

                case "n":
                case "num":
                    if (parsedInput.Length >= 2 && int.TryParse(parsedInput[1], out parsedInt))
                    {
                        numTitles = parsedInt;
                        Console.WriteLine("Will now print " + numTitles + " titles per entry");
                        break;
                    }
                    else
                    {
                        goto default;
                    }

                case "v":
                case "ver":
                case "verbose":
                    verbose = !verbose;
                    if (verbose)
                    {
                        Console.WriteLine("Verbose flag turned on");
                    }
                    else
                    {
                        Console.WriteLine("Verbose flag turned off");
                    }
                    break;

                default:
                    Console.WriteLine(InvalidInputText);
                    break;
            }

            return true;
        }

        /// <summary>
        /// Generates numTitles number of titles with a given mood
        /// </summary>
        /// <param name="mood">specifies how positive/negative the titles must be</param>
        private static void Generate(int mood)
        {
            HashSet<string> titles = new HashSet<string>();

            mood = Math.Max(mood, -MaxMood);
            mood = Math.Min(mood, MaxMood);

            for (int i = 0; i < numTitles; ++i)
            {
                WordData title = titleGenerator.GenerateTitle();

                bool timedOut = true;
                // Keep generating titles until we find one with the correct mood, 
                // correct number of syllables, and has not been printed yet
                for (int j = 0; j < MaxIters; ++j)
                {
                    title = titleGenerator.GenerateTitle();

                    if (Math.Abs(title.Mood - mood) <= Tolerance &&
                        CheckSyllables(title) &&
                        !titles.Contains(title.Word))
                    {
                        timedOut = false;
                        break;
                    }
                }

                if (timedOut)
                {
                    Console.WriteLine("This request timed out.  Please either relax the requirements or add more words to TitleGen.xml");
                    return;
                }

                if (verbose)
                {
                    Console.WriteLine(title.Word + ": mood = " + title.Mood + ", syllables = " + title.Syllables);
                }
                else
                {
                    Console.WriteLine(title.Word);
                }

                titles.Add(title.Word);
            }
        }

        /// <summary>
        /// Prints HelpText
        /// </summary>
        private static void Help()
        {
            foreach (string line in HelpText)
            {
                Console.WriteLine(line);
            }
        }

        /// <summary>
        /// Checks if a title has the correct number of syllables
        /// </summary>
        /// <param name="title">the title to check</param>
        /// <returns>true if the title has a valid number of syllables</returns>
        private static bool CheckSyllables(WordData title)
        {
            if (Syllables == null)
            {
                return true;
            }

            return title.Syllables == Syllables || (Syllables >= MaxSyllables && title.Syllables > Syllables);
        }
    }
}
