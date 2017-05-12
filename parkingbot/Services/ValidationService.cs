using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
            }

            return true;
        }

        public string ReturnWhatYouTyped(Dictionary<string, string> postData)
        {
            var message = "/";

            message += postData["command"].Replace("%2F", string.Empty).Trim();
            message += " ";
            message += string.Join(" ", postData["text"].Split('+')).Trim();

            return message;
        }

        public Dictionary<string, string> ParsePostData(string text)
        {
            var dictionary = text.Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('='))
                .ToDictionary(split => split[0], split => split[1]);

            return dictionary;
        }
    }

    public interface IValidationService
    {
        bool ValidDate(string dateString);
        bool IsValidLaisvosVietosParameters(Dictionary<string, string> postData);
        string ReturnWhatYouTyped(Dictionary<string, string> postData);
        Dictionary<string, string> ParsePostData(string text);
    }
}