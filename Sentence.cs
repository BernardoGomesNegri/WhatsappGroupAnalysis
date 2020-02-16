using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace WhatsAppGroupAnalysis
{

    class Sentence
    {
        private static readonly Regex CleanEmoji = new Regex(@"\p{Cs}", RegexOptions.Compiled);

        private static readonly char[] Delimiters = new char[] { ' ', '\r', '\n' };

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
                Words = 0;
                return;
            }

            string clean = CleanEmoji.Replace(What, string.Empty);

            EmojiCount += (What.Length - clean.Length) / 2;
            
            Lenght = clean.Length;
            Words = clean.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries).Length;

            MomentCategory = Moment.GetCategory();

        }
        
        public int Lenght { get; private set; }
        public int Words { get; private set; }

        public MomentCategory MomentCategory { get; private set; }
    }
}
