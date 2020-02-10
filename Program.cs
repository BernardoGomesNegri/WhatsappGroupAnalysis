using System;
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

                var lines = File.ReadAllLines(file, System.Text.Encoding.UTF8);
                Console.WriteLine($"Lidas {lines.Length} linhas.");

                var regex = new Regex(@"\d\d/\d\d/\d{4}\s\d\d:\d\d\s-\s", RegexOptions.Compiled);

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
                        var whoAndWhat = currentText.Substring(19);
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
                                Moment = DateTime.ParseExact(currentText.Substring(0, 16), "dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture),
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
                    s.Calculate();
                }

                Console.WriteLine($"{sentences.Count} sentenças lidas.");

                // Distintas Pessoas e o número de senteças
                var byPerson = sentences.GroupBy(s => s.Who.ToLowerInvariant()).OrderByDescending(g => g.Count()).ToArray();

                var sb = new StringBuilder();
                sb.AppendLine("Quem\tMsgs\tPPM\tLPP\tEPM\tMPM\tFreqCoj\tFreqMor\tFreqAft\tFreqNight");
                
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

                    sb.AppendLine($"{g.Key}\t{total}\t{ppm:r}\t{lpp:r}\t{epm:r}\t{mpm:r}\t{freqCoj:r}\t{freqMor:r}\t{freqAft:r}\t{freqNight:r}");
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
    }
}
