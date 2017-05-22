using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using parkingbot.Models;

namespace parkingbot.Services
{
    public class ValidationService : IValidationService
    {
        public ValidationService()
        {

        }

        public bool ValidDate(string dateString)
        {
            DateTime dateTime;

            return DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out dateTime);
        }

        public bool IsValidLaisvosVietosParameters(Dictionary<string, string> postData)
        {
            if (postData.ContainsKey("text"))
            {
                var data = postData["text"].Trim().Split('+');

                if (data.Length != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsValidLaisvaImuParameters(Dictionary<string, string> postData)
        {
            if (postData.ContainsKey("text"))
            {
                var data = postData["text"].Trim().Split('+');

                if (data.Length != 6)
                {
                    return false;
                }

                if (data[0].ToUpper() != "SEIMYNISKIU" && data[0].ToUpper() != "JUOZAPAVICIAUS")
                {
                    return false;
                }

                if (data[2].ToUpper() != "NUO")
                {
                    return false;
                }

                if (!ValidDate(data[3]))
                {
                    return false;
                }

                if (data[4].ToUpper() != "IKI")
                {
                    return false;
                }

                if (!ValidDate(data[5]))
                {
                    return false;
                }

                var dateFrom = DateTime.ParseExact(data[3], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var dateTo = DateTime.ParseExact(data[5], "yyyy-MM-dd", CultureInfo.InvariantCulture);

                var dateNow = DateTime.ParseExact(
                    DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    "yyyy-MM-dd", CultureInfo.InvariantCulture
                );

                if (dateFrom > dateTo)
                {
                    return false;
                }

                if (dateFrom < dateNow)
                {
                    return false;
                }

                if (data[1].Length > 10)
                {
                    return false;
                }
            }

            return true;
        }

        public Dictionary<string, string> ParsePostData(string text)
        {
            var dictionary = text.Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('='))
                .ToDictionary(split => split[0], split => split[1]);

            return dictionary;
        }

        public bool AvailabilityRowExists(List<Availability> availabilities, Availability row)
        {
            return availabilities.Any(availability => availability.Id == row.Id);
        }

        public bool LogsRowExists(List<Logs> logs, Logs row)
        {
            return logs.Any(log => log.Id == row.Id);
        }
    }

    public interface IValidationService
    {
        bool ValidDate(string dateString);

        bool IsValidLaisvosVietosParameters(Dictionary<string, string> postData);

        bool IsValidLaisvaImuParameters(Dictionary<string, string> postData);

        Dictionary<string, string> ParsePostData(string text);

        bool AvailabilityRowExists(List<Availability> availabilities, Availability row);

        bool LogsRowExists(List<Logs> logs, Logs row);
    }
}