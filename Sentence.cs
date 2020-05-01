using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WhatsAppGroupAnalysis
{

    class Sentence
    {
        private static readonly Regex CleanEmoji = new Regex(@"\p{Cs}", RegexOptions.Compiled);

        private static readonly char[] DelimitersOne = { ' ', '\r', '\n'};

        private static readonly char[] DelimitersTwo = { ' ', ' ', '\r', '\n', ';', '.', ',', '-', '–', '_', '¯', '?', '!', '\t', '/', '\\', '(', ')', '{', '}', '[', ']', '@', '=', '*', '\'', '"', '+', '“', '”', '>', '<', '`', };
        public DateTime Moment { get; set; }
        public string Who { get; set; }
        public string What { get; set; }

        public bool IsOnlyImage { get; private set; }

        public int EmojiCount { get; private set; }

        public void Calculate(string langString)
        {
            if (What.Equals(langString, StringComparison.CurrentCultureIgnoreCase))
            {
                
                IsOnlyImage = true;
                Lenght = 0;
                WordsCount = 0;
                return;
            }

            var clean = CleanEmoji.Replace(What, string.Empty);

            EmojiCount += (What.Length - clean.Length) / 2;
            
            Lenght = clean.Length;
            var resultsFirst = clean.ToLowerInvariant().Split(DelimitersOne, StringSplitOptions.RemoveEmptyEntries);
            var l = new List<string>();
            foreach (var item in resultsFirst)
            {
                if(item.StartsWith("http://"))
                {
                    continue;
                }
                if (item.StartsWith("https://"))
                {
                    continue;
                }
                foreach (var w in item.Split(DelimitersTwo, StringSplitOptions.RemoveEmptyEntries))
                {
                    l.Add(w);
                }
                
            }
            Words = l.ToArray();

            WordsCount = Words.Length;
            

            MomentCategory = Moment.GetCategory();

        }
        
        public int Lenght { get; private set; }
        public string[] Words { get; private set; } = Array.Empty<string>();
        public int WordsCount { get; private set; }

        public MomentCategory MomentCategory { get; private set; }
    }
}
