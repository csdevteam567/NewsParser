using System.Net;

namespace NewsParser.Messenger
{
    public class TelegramSender : IMessengerSender
    {
        private static string token;
        private static string chatId;

        public TelegramSender(string tkn, string chtId)
        {
            token = tkn;
            chatId = chtId;
        }

        public void SendMessage(Message msg)
        {
            string message = msg.Title + "\n" + msg.Body;
            WebRequest req = WebRequest.Create("https://api.telegram.org/bot" + token + "/sendMessage?chat_id=" + chatId + "&text=" + message);
            req.UseDefaultCredentials = true;

            var result = req.GetResponse();
            req.Abort();
        }

    }
}
