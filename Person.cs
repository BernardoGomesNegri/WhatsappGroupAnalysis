namespace WhatsAppGroupAnalysis
{
    class Person
    {
        public string Name { get; set; }

        public int Total { get; internal set; }
        public int TotalLenght { get; internal set; }
        public int TotalWords { get; internal set; }
        public double WordsPerMessage { get; internal set; }
        public double LettersPerWord { get; internal set; }
        internal double FrequenceNight { get; set; }
        internal double FrequenceAfternoon { get; set; }
        internal double FrequenceMorning { get; set; }
        internal double FrequenceCorujão { get; set; }
        internal double EmojisPerMessage { get; set; }
        internal double MediaPerMessage { get; set; }
        internal double DaysPresent { get; set; }

        public override string ToString()
        {
            return $"{Name}\t{Total}\t{WordsPerMessage:r}\t{LettersPerWord:r}\t{EmojisPerMessage:r}\t{MediaPerMessage:r}\t" +
                $"{FrequenceCorujão:r}\t{FrequenceMorning:r}\t{FrequenceAfternoon:r}\t{FrequenceNight:r}\t{DaysPresent}";
        }
    }
}
