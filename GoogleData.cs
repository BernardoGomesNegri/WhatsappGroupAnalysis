using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace WhatsAppGroupAnalysis
{
    public class GoogleData
    {
        public string MimeType { get; set; }
        public string Data { get; set; }

        private string[] GetLines()
        {
            return Data.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] GetLines(string fileName, Encoding encoding)
        {
            var all = File.ReadAllText(fileName, encoding);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var data = JsonSerializer.Deserialize<GoogleData>(all, options);
            return data.GetLines();
        }
    }
}