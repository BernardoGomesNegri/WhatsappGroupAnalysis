﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WhatsAppGroupAnalysis
{

    class Program
    {
        public static string LangString = "<Arquivo de mídia oculto>";

        public static string LangRegex = @"\d\d/\d\d/\d{4}\s\d\d:\d\d\s-\s";

        public static string LangDateFormat = "dd/MM/yyyy HH:mm";

        private static CultureInfo LangCulture = CultureInfo.GetCultureInfo("pt-br");
        static int Main(string[] args)
        {
            try
            {
                if (args.Length <= 0)
                {
                    Console.WriteLine("Informe o arquivo de conversa do WhatsApp.");
                    return 1;
                }
                var file = args[0];

                if (!File.Exists(file))
                {
                    Console.WriteLine($"Arquivo '{file}' não existe.");
                    return 2;
                }

                //Checa língua

                if (args.Length > 1)
                {
                    if(args[1].Substring(0, 5).ToLowerInvariant() == "lang:")
                    {
                        var langCode = args[1].Substring(5);
                        switch (langCode)
                        {
                            case "pt": 
                                break;
                            case "en": 
                                LangString = "<Media omitted>";
                                LangRegex = @"\d{1,2}\/\d{1,2}\/\d\d,\s\d{1,2}:\d{2}\s[A,P]M\s-\s";
                                LangDateFormat = "M/d/yy, h:mm tt";
                                LangCulture = CultureInfo.GetCultureInfo("en-us");
                                break;
                            default:
                                Console.WriteLine($"Linguagem {langCode} não suportada");
                                return 3;
                        }
                    }
                }
                

                if (args.Length > 1)
                {
                    if(args[1].StartsWith("format:"))
                    {

                    }
                }

                var lines = File.ReadAllLines(file, System.Text.Encoding.UTF8);
                Console.WriteLine($"Lidas {lines.Length} linhas.");

                var regex = new Regex(LangRegex, RegexOptions.Compiled);

                Sentence currentSentence = null;
                var sentences = new List<Sentence>();

                var current = 0;
                while (current < lines.Length)
                {
                    var currentText = lines[current];

                    // exemplo de linha
                    // 02/02/2020 - Pessoa: Lorem Ipsum
                    if (currentText.Length < 19)
                    {
                        // Deve ser continuação de um registro anterior
                        if (currentSentence != null)
                        {
                            currentSentence.What += " " + currentText;
                        }
                    }
                    else if (regex.IsMatch(currentText))
                    {
                        // Achei um novo registro!
                        var startOfWhoAndWhat = currentText.IndexOf(" - ");

                        var whoAndWhat = currentText.Substring(startOfWhoAndWhat + 3);
                        var posSepator = whoAndWhat.IndexOf(':');
                        if (posSepator > 0)
                        {
                            var who = whoAndWhat.Substring(0, posSepator);
                            var what = whoAndWhat.Substring(posSepator + 2);

                            // Criar um novo registro

                            if (currentSentence != null)
                            {
                                sentences.Add(currentSentence);
                            }

                            currentSentence = new Sentence
                            {
                                Moment = DateTime.ParseExact(currentText.Substring(0, startOfWhoAndWhat), LangDateFormat, LangCulture),
                                Who = who,
                                What = what
                            };
                            
                        }
                    }
                    else
                    {
                        // Também deve ser continuação de um registro anterior, mas com texto grande
                        if (currentSentence != null)
                        {
                            currentSentence.What += " " + currentText;
                        }
                    }

                    current++;
                }

                if (currentSentence != null)
                {
                    sentences.Add(currentSentence);
                }

                foreach(var s in sentences)
                {
                    s.Calculate(LangString);
                }

                Console.WriteLine($"{sentences.Count} sentenças lidas.");

                // Distintas Pessoas e o número de senteças
                var byPerson = sentences.GroupBy(s => s.Who.ToLowerInvariant()).OrderByDescending(g => g.Count()).ToArray();

                var sb = new StringBuilder();
                sb.AppendLine("Quem\tMsgs\tPPM\tLPP\tEPM\tMPM\tFreqCoj\tFreqMor\tFreqAft\tFreqNight\tDiasPresente");
                
                foreach(var g in byPerson)
                {
                    double total = g.Count();
                    var totalLenght = g.Sum(s => s.Lenght);
                    var totalWords = g.Sum(s => s.Words);

                    // Letras Por Palavra
                    var lpp = totalLenght / (double)totalWords;

                    // Palavras por Mensagem
                    var ppm = totalWords / total;

                    // Midia por Mensagem
                    var mpm = g.Count(s => s.IsOnlyImage) / total;

                    // Emoji por Mensagem
                    var epm = g.Sum(s => s.EmojiCount) / total;

                    // Frequencia de Corujão
                    var freqCoj = g.Count(s => s.MomentCategory == MomentCategory.Corujão) / total;

                    // Frequencia de Manhã
                    var freqMor = g.Count(s => s.MomentCategory == MomentCategory.Morning) / total;

                    // Frequencia de Tarde
                    var freqAft = g.Count(s => s.MomentCategory == MomentCategory.Afternoon) / total;

                    // Frequencia de Noite
                    var freqNight = g.Count(s => s.MomentCategory == MomentCategory.Night) / total;

                    //Dias com participação
                    var daysPresent = g.Select(s => s.Moment.Date).Distinct().Count();

                    sb.AppendLine($"{g.Key}\t{total}\t{ppm:r}\t{lpp:r}\t{epm:r}\t{mpm:r}\t{freqCoj:r}\t{freqMor:r}\t{freqAft:r}\t{freqNight:r}\t{daysPresent}");
                }

                var fi = new FileInfo(file);
                var outFile = Path.Combine(fi.DirectoryName, fi.Name + ".out.txt");
                File.WriteAllText(outFile, sb.ToString(), Encoding.UTF8);

                Console.WriteLine($"Resultados em '{outFile}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1000;
            }
            return 0;
        }

        private static int ParseParms (string [] args, out string lang, out string reportFormat, out string fileName)
        {
            lang = "pt";
            reportFormat = "excel";
            fileName = null;

            if (args.Length >= 1)
            {
                fileName = args[0];
            }
            else
            {
                Console.WriteLine("O primeiro parâmetro deve ser o caminho do arquivo");
                return 1;
            }

            for (int i = 1; i < args.Length; i++)
            {
                var p = args[i];

                if (p.Substring(0, 5).ToLowerInvariant() == "lang:")
                {
                    var langCode = p.Substring(5);
                    switch (langCode)
                    {
                        case "pt":
                            break;
                        case "en":
                            lang = "en";
                            break;
                        default:
                            Console.WriteLine($"Linguagem {langCode} não suportada");
                            return 3;
                    }
                }
                else if (p.Substring(0, 7).ToLowerInvariant() == "format:")
                {

                }
            }
        }
    }
}
