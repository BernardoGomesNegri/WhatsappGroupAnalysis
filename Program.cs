using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace WhatsAppGroupAnalysis
{
    
    
    class Program
    {
       

        private static CultureInfo _langCulture = CultureInfo.GetCultureInfo("pt-br");

        private static int Main(string[] args)
        {
            try
            {
                var langString = "<Arquivo de mídia oculto>";
                var langRegex = @"\d\d/\d\d/\d{4}\s\d\d:\d\d\s-\s";
                var langDateFormat = "dd/MM/yyyy HH:mm";
                var startOfWhoAndWhatSeparator = " - ";
                
                //Checa língua
                var res = ParseParameters(args, out var lang, out var reportFormat, out var file, out var platform);
                if (res != 0)
                {
                    return res;
                }

                if (platform == Platform.WhatsApp)
                {
                    switch (lang)
                    {
                        case Language.Pt:
                            break;
                        case Language.En:
                            langString = "<Media omitted>";
                            langRegex = @"\d{1,2}\/\d{1,2}\/\d\d,\s\d{1,2}:\d{2}\s[A,P]M\s-\s";
                            langDateFormat = "M/d/yy, h:mm tt";
                            _langCulture = CultureInfo.GetCultureInfo("en-us");
                            break;
                    }
                }
                else
                {
                    langRegex = @"\d{2}:\d{2}:\d{2}\.\d{3},\d{2}:\d{2}:\d{2}\.\d{3}";
                    startOfWhoAndWhatSeparator = "\n";
                }

                var lines = GetLines(file, platform);

                Console.WriteLine($"Lidas {lines.Length} linhas.");

                var regex = new Regex(langRegex, RegexOptions.Compiled);

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
                        
                        var startOfWhoAndWhat = currentText.IndexOf(startOfWhoAndWhatSeparator, StringComparison.Ordinal);

                        var whoAndWhat = currentText.Substring(startOfWhoAndWhat + startOfWhoAndWhatSeparator.Length);
                        var posSeparator = whoAndWhat.IndexOf(':');
                        if (posSeparator > 0)
                        {
                            var who = whoAndWhat.Substring(0, posSeparator);
                            var what = whoAndWhat.Substring(posSeparator + 2);

                            // Criar um novo registro

                            if (currentSentence != null)
                            {
                                sentences.Add(currentSentence);
                            }

                            DateTime moment;
                            if (platform == Platform.WhatsApp)
                            {
                                moment = DateTime.ParseExact(currentText.Substring(0, startOfWhoAndWhat),
                                    langDateFormat, _langCulture);
                                
                            }
                            else
                            {
                                Debug.Assert(platform == Platform.GoogleMeet);

                                var m = DateTime.ParseExact(currentText.Substring(0, 8), "HH:mm:ss", _langCulture);
                                var t = DateTime.Today;
                                moment = new DateTime(t.Year, t.Month, t.Day, m.Hour, m.Minute, m.Second, DateTimeKind.Unspecified);
                            }

                            currentSentence = new Sentence
                            {
                                Moment = moment,
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

                var popularWords = new Dictionary<string, int>();
                foreach (var s in sentences)
                {
                    s.Calculate(langString);
                    foreach (var w in s.Words)
                    {
                        if (popularWords.TryGetValue(w, out var count))
                        {
                            popularWords[w] = ++count;
                        }
                        else
                        {
                            popularWords.Add(w, 1);
                        }
                    }
                }

                Console.WriteLine($"{sentences.Count} sentenças lidas.");

                // Distintas Pessoas e o número de sentenças
                var byPerson = sentences.GroupBy(s => s.Who.ToLowerInvariant()).OrderByDescending(g => g.Count()).ToArray();

                var persons = new List<Person>();

                foreach (var g in byPerson)
                {
                    var p = new Person
                    {
                        Total = g.Count(),
                        Name = g.Key,
                        TotalLenght = g.Sum(s => s.Lenght),
                        TotalWords = g.Sum(s => s.WordsCount)
                    };



                    // Letras Por Palavra
                    p.LettersPerWord = p.TotalLenght / (double)p.TotalWords;

                    // Palavras por Mensagem
                    p.WordsPerMessage = p.TotalWords / (double)p.Total;

                    // Mídia por Mensagem
                    p.MediaPerMessage = g.Count(s => s.IsOnlyImage) / (double)p.Total;

                    // Emoji por Mensagem
                    p.EmojisPerMessage = g.Sum(s => s.EmojiCount) / (double)p.Total;

                    // Frequência de Corujão
                    p.FrequenceCorujão = g.Count(s => s.MomentCategory == MomentCategory.Corujão) / (double)p.Total;

                    // Frequência de Manhã
                    p.FrequenceMorning = g.Count(s => s.MomentCategory == MomentCategory.Morning) / (double)p.Total;

                    // Frequência de Tarde
                    p.FrequenceAfternoon = g.Count(s => s.MomentCategory == MomentCategory.Afternoon) / (double)p.Total;

                    // Frequência de Noite
                    p.FrequenceNight = g.Count(s => s.MomentCategory == MomentCategory.Night) / (double)p.Total;

                    //Dias com participação
                    p.DaysPresent = g.Select(s => s.Moment.Date).Distinct().Count();

                    persons.Add(p);

                }

                switch (reportFormat)
                {
                    case ReportFormat.Excel:
                        ExportExcel(persons, popularWords, file);
                        break;
                    case ReportFormat.TabSeparated:
                        ExportTsv(persons, file);
                        break;

                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1000;
            }
            return 0;
        }

        private static string[] GetLines(string fileName, Platform platform)
        {
            if (platform == Platform.WhatsApp)
            {
                return File.ReadAllLines(fileName, Encoding.UTF8);
            }

            Debug.Assert(platform == Platform.GoogleMeet);

            return GoogleData.GetLines(fileName, Encoding.UTF8);
        }

        private static void ExportTsv(List<Person> persons, string file)
        {
            var fi = new FileInfo(file);
            var outFile = Path.Combine(fi.DirectoryName, fi.Name + ".out.txt");

            var sb = new StringBuilder();
            sb.AppendLine("Quem\tMsgs\tPPM\tLPP\tEPM\tMPM\tFreqCoj\tFreqMor\tFreqAft\tFreqNight\tDiasPresente");
            foreach (var p in persons)
            {
                sb.AppendLine(p.ToString());
            }

            File.WriteAllText(outFile, sb.ToString(), Encoding.UTF8);

            Console.WriteLine($"Resultados em '{outFile}'");
        }

        private static void ExportExcel(List<Person> persons, Dictionary<string, int> popularWords, string file)
        {
            using var excelFile = new ExcelPackage();
            var ws = excelFile.Workbook.Worksheets.Add("Pessoas");
            ws.Cells[1, 1].Value = "Pessoas";

            //Header
            ws.Cells[3, 1].Value = "Quem";
            ws.Cells[3, 2].Value = "Mensagens";
            ws.Cells[3, 3].Value = "Palavras por Mensagem";
            ws.Cells[3, 4].Value = "Letras por Palavra";
            ws.Cells[3, 5].Value = "Emojis por Mensagem";
            ws.Cells[3, 6].Value = "Mídias por Mensagem";
            ws.Cells[3, 7].Value = "Frequência Corujão";
            ws.Cells[3, 8].Value = "Frequência Manhã";
            ws.Cells[3, 9].Value = "Frequência Tarde";
            ws.Cells[3, 10].Value = "Frequência Noite";
            ws.Cells[3, 11].Value = "Dias Presente";

            var r = 4;
            foreach (var p in persons)
            {
                ws.Cells[r, 1].Value = p.Name;
                ws.Cells[r, 2].Value = p.Total;
                ws.Cells[r, 3].Value = p.WordsPerMessage;
                ws.Cells[r, 4].Value = p.LettersPerWord;
                ws.Cells[r, 5].Value = p.EmojisPerMessage;
                ws.Cells[r, 6].Value = p.MediaPerMessage;
                ws.Cells[r, 7].Value = p.FrequenceCorujão;
                ws.Cells[r, 8].Value = p.FrequenceMorning;
                ws.Cells[r, 9].Value = p.FrequenceAfternoon;
                ws.Cells[r, 10].Value = p.FrequenceNight;
                ws.Cells[r, 11].Value = p.DaysPresent;
                r++;
            }
            var wsWords = excelFile.Workbook.Worksheets.Add("Palavras Comuns");
            wsWords.Cells[1, 1].Value = "Palavras mais comuns";

            //Header
            wsWords.Cells[3, 1].Value = "Palavra";
            wsWords.Cells[3, 2].Value = "Frequência";

            r = 4;
            foreach (var item in popularWords.OrderByDescending(kv => kv.Value))
            {
                wsWords.Cells[r, 1].Value = item.Key;
                wsWords.Cells[r, 2].Value = item.Value;
                r++;
            }

            var fi = new FileInfo(file);
            var outFile = Path.Combine(fi.DirectoryName, fi.Name + ".out.xlsx");
            excelFile.SaveAs(new FileInfo(outFile));



            Console.WriteLine($"Resultados em '{outFile}'");
        }

        private static int ParseParameters(string[] args, out Language lang, out ReportFormat reportFormat, out string fileName, out Platform platform)
        {
            lang = Language.Pt;
            reportFormat = ReportFormat.Excel;
            fileName = null;
            platform = Platform.WhatsApp;

            if (args.Length >= 1)
            {
                fileName = args[0];
            }
            else
            {
                Console.WriteLine("O primeiro parâmetro deve ser o caminho do arquivo");
                return 1;
            }

            for (var i = 1; i < args.Length; i++)
            {
                var p = args[i];

                if (p.Substring(0, 5).ToLowerInvariant() == "lang:")
                {
                    var langCode = p.Substring(5);
                    switch (langCode)
                    {
                        case "pt":
                            lang = Language.Pt;
                            break;
                        case "en":
                            lang = Language.En;
                            break;
                        default:
                            Console.WriteLine($"Linguagem {langCode} não suportada");
                            return 3;
                    }
                }
                else if (p.Substring(0, 7).ToLowerInvariant() == "format:")
                {
                    var formatString = p.Substring(7).ToLowerInvariant();
                    switch (formatString)
                    {
                        case "tsv":
                        case "txt":
                        case "text":
                            reportFormat = ReportFormat.TabSeparated;
                            break;
                        case "xls":
                        case "xlsx":
                        case "excel":
                            reportFormat = ReportFormat.Excel;
                            break;
                        default:
                            Console.WriteLine($"Formato {formatString} não suportado");
                            return 4;
                    }
                }

                else if (p.Substring(0, 9).ToLowerInvariant() == "platform:")
                {
                    var formatString = p.Substring(9).ToLowerInvariant();
                    switch (formatString)
                    {
                        case "whatsapp":
                        case "ws":
                            platform = Platform.WhatsApp;
                            break;
                        case "google_meet":
                        case "google-meet":
                        case "googlemeet":
                        case "google":
                        case "meet":
                            platform = Platform.GoogleMeet;
                            break;
                        default:
                            Console.WriteLine($"Plataforma {formatString} não suportada");
                            return 5;
                    }
                }
            }
            return 0;
        }
    }
}
