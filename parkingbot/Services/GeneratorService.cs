using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using parkingbot.Models;

namespace parkingbot.Services
{
    public class GeneratorService : IGeneratorService
    {
        public GeneratorService()
        {

        }

        public string ReturnWhatYouTyped(Dictionary<string, string> postData)
        {
            var message = "/";

            message += postData["command"].Replace("%2F", string.Empty).Trim();
            message += " ";
            message += string.Join(" ", postData["text"].Split('+')).Trim();

            return message;
        }

        public string UniqueAvailabilityId(string location, string spot, DateTime dateFrom, DateTime dateTo)
        {
            var array = new List<string>
            {
                location,
                spot,
                dateFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                dateTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };

            return Base64Encode(string.Join("+", array.ToArray()));
        }

        public string UniqueLogsId(string username, string location, string spot, DateTime dateFrom, DateTime dateTo, string action)
        {
            var array = new List<string>
            {
                username,
                location,
                spot,
                dateFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                dateTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                action
            };

            return Base64Encode(string.Join("+", array.ToArray()));
        }

        private static string Base64Encode(string plainText) {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public string GenerateTable(List<Row> rows)
        {
            var table = string.Empty;

            foreach (var row in rows)
            {
                for (var b = 0; b < row.Column.Count; b++)
                {
                    var maxColumnWidth = GetMaxColumnWidth(rows, b);
                    var fillTheGap = maxColumnWidth - row.Column[b].Length;

                    if (fillTheGap > 0)
                    {
                        row.Column[b] = row.Column[b] + new string(' ', fillTheGap);
                    }
                }
            }

            var tableWidth = GetTableWidth(rows);

            table += new string('-', tableWidth) + "\n";

            for (var i = 0; i < rows.Count; i++)
            {
                if (i == 1)
                {
                    table += new string('-', tableWidth) + "\n";
                }

                table += "| ";
                table += string.Join(" | ", rows[i].Column);
                table += " |\n";
            }

            table += new string('-', tableWidth) + "\n";

            return table;
        }

        private static int GetTableWidth(IReadOnlyList<Row> rows)
        {
            var tableWidth = 0;

            tableWidth += "| ".Length;

            foreach (var column in rows[0].Column)
            {
                tableWidth += column.Length;
            }

            tableWidth += (rows[0].Column.Count - 1) * (" | ".Length);

            tableWidth += " |".Length;

            return tableWidth;
        }

        private static int GetMaxColumnWidth(IEnumerable<Row> rows, int i)
        {
            return rows.Select(row => row.Column[i].Length).Concat(new[] {0}).Max();
        }
    }

    public interface IGeneratorService
    {
        string ReturnWhatYouTyped(Dictionary<string, string> postData);

        string UniqueAvailabilityId(string location, string spot, DateTime dateFrom, DateTime dateTo);

        string UniqueLogsId(string username, string location, string spot, DateTime dateFrom, DateTime dateTo,
            string action);

        string GenerateTable(List<Row> rows);
    }
}