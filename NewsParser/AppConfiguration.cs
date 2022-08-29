using System.Configuration;

namespace NewsParser
{
    public class AppConfiguration
    {
        private int parseInterval;
        private string connectionString;
        private string telegramToken;
        private string telegramChatId;

        public AppConfiguration()
        {
            readConfigurations();
        }

        public int ParseInterval
        {
            get
            {
                return parseInterval;
            }
        }

        public string ConnectionString
        {
            get
            {
                return connectionString;
            }
        }

        public string TelegramToken
        {
            get
            {
                return telegramToken;
            }
        }

        public string TelegramChatId
        {
            get
            {
                return telegramChatId;
            }
        }

        private void readConfigurations()
        {
            parseInterval = int.Parse(ConfigurationManager.AppSettings.Get("parseInterval"));
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            telegramToken = ConfigurationManager.AppSettings.Get("telegramToken");
            telegramChatId = ConfigurationManager.AppSettings.Get("telegramChatId");
        }
    }
}
