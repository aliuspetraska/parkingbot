using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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
    }

    public interface IGeneratorService
    {
        string ReturnWhatYouTyped(Dictionary<string, string> postData);
        string UniqueAvailabilityId(string location, string spot, DateTime dateFrom, DateTime dateTo);
        string UniqueLogsId(string username, string location, string spot, DateTime dateFrom, DateTime dateTo,
            string action);
    }
}