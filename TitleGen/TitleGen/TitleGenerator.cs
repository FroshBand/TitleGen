namespace TitleGen
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Generates titles from a list of FirstWords and SecondWords deserialized from XML
    /// </summary>
    [Serializable]
    [XmlRoot("TitleGen")]
    public class TitleGenerator
    {
        [XmlArray("FirstWords")]
        [XmlArrayItem("WordData")]
        public WordData[] FirstWords { get; set; }

        [XmlArray("SecondWords")]
        [XmlArrayItem("WordData")]
        public WordData[] SecondWords { get; set; }

        static readonly char[] vowels = { 'A', 'E', 'I', 'O', 'U', 'a', 'e', 'i', 'o', 'u' };

        static Random rand = new Random(DateTime.Now.Millisecond);

        /// <summary>
        /// Generates a title by combining a word from FirstWords and SecondWords
        /// </summary>
        /// <returns>the generated title</returns>
        public WordData GenerateTitle()
        {
            while (true)
            {
                WordData firstWord = FirstWords[rand.Next(0, FirstWords.Length)];
                WordData secondWord = SecondWords[rand.Next(0, SecondWords.Length)];

                // If firstWord and secondWord are the same, try again
                if (!firstWord.Word.Contains(secondWord.Word) && !secondWord.Word.Contains(firstWord.Word))
                {
                    // Remove shared double characters 
                    string secondWordProcessed = (char.ToLower(firstWord.Word[firstWord.Word.Length - 1]) == char.ToLower(secondWord.Word[0])) ?
                        secondWord.Word.Substring(1) :
                        secondWord.Word;

                    // If firstWord ends in a vowel, remove starting vowels from SecondWord
                    if (IsVowel(firstWord.Word[firstWord.Word.Length - 1]))
                    {
                        while (IsVowel(secondWordProcessed[0]))
                        {
                            secondWordProcessed = secondWordProcessed.Substring(1);
                        }
                    }

                    return new WordData
                    {
                        Word = firstWord.Word + secondWordProcessed,
                        Mood = firstWord.Mood + secondWord.Mood,
                        Syllables = firstWord.Syllables + secondWord.Syllables
                    };
                }
            }
        }

        /// <summary>
        /// Checks if a character is a vowel
        /// </summary>
        /// <param name="letter">the character to check</param>
        /// <returns>returns true if letter is a vowel</returns>
        private bool IsVowel(char letter)
        {
            foreach (char vowel in vowels)
            {
                if (letter == vowel)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Class organizing the information associated with a single word
    /// </summary>
    [Serializable]
    public class WordData
    {
        /// <summary>
        /// The word as a string 
        /// </summary>
        [XmlElement("Word")]
        public string Word { get; set; }

        /// <summary>
        /// The feeling associated with the word, from -3 (most negative) to 3 (most positive)
        /// </summary>
        [XmlElement("Mood")]
        public int Mood { get; set; }

        /// <summary>
        /// The number of syllables in the word
        /// </summary>
        [XmlElement("Syllables")]
        public int Syllables { get; set; }
    }
}
