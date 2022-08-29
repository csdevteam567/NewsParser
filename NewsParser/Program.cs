using HtmlAgilityPack;
using NewsParser;
using NewsParser.Messenger;
using NewsParser.Model;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Net;

class Program
{
    private static ConsoleKeyInfo option;
    private static AppConfiguration config;
    private static DatabaseAccess dbAccess;
    private static Object lockObj = new Object();
    private static TelegramSender tlg;

    static Program()
    {
        config = new AppConfiguration();
        dbAccess = new DatabaseAccess(config.ConnectionString);
        tlg = new TelegramSender(config.TelegramToken, config.TelegramChatId);
    }

    private static void ParserHtmlWorkerFunc(ParserConfig config)
    {
        HtmlWeb web = new HtmlWeb();
        try
        {
            HtmlDocument doc = web.Load(config.BaseUrl);
            var articleNodes = doc.DocumentNode.SelectNodes(config.ArticleTag);

            foreach (var articleNode in articleNodes)
            {
                HtmlDocument article = new HtmlDocument();
                article.LoadHtml(articleNode.InnerHtml);

                var titleNode = article.DocumentNode.SelectSingleNode(config.TitleTag);
                var contentNode = article.DocumentNode.SelectSingleNode(config.ContentTag);
                string title = "";
                string content = "";
                if (titleNode != null)
                {
                    title = titleNode.InnerText;
                }

                if (contentNode != null)
                {
                    content = contentNode.InnerText;
                }

                if (!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(content))
                {
                    lock (lockObj)
                    {
                        dbAccess.AddNewsRecord(new News()
                        {
                            Title = title,
                            Content = content
                        });

                        tlg.SendMessage(new Message()
                        {
                            Title = title,
                            Body = content
                        });
                    }
                }
            }
        }catch(Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static void ParserJsonWorkerFunc(ParserConfig config)
    {
        CookieContainer cookies = new CookieContainer();
        HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(config.BaseUrl);
        webRequest.UserAgent = @"PostmanRuntime/7.29.2";
        Uri uri = new Uri(config.BaseUrl);
        webRequest.Host = uri.Host;
        webRequest.Headers.Add("Postman-Token", "d74f2c26-4c9b-4c66-83a1-f6d135506c4b");
        webRequest.Headers.Add("Accept", "*/*");
        webRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
        webRequest.Headers.Add("Connection", "keep-alive");

        webRequest.Method = "GET";
        webRequest.CookieContainer = cookies;

        string resultString;
        try
        {
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                var responceStream = Decompress(webResponse.GetResponseStream());
                using (StreamReader streamReader = new StreamReader(responceStream))
                {
                    resultString = streamReader.ReadToEnd();

                    Console.WriteLine(resultString);
                }
            }
            dynamic resultObject = JsonConvert.DeserializeObject(resultString);
            var articles = resultObject[config.ArticleTag];
            foreach (var article in articles)
            {
                string title = "";
                string content = "";
                title = article[config.TitleTag];
                content = article[config.ContentTag];

                if (!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(content))
                {
                    lock (lockObj)
                    {
                        dbAccess.AddNewsRecord(new News()
                        {
                            Title = title,
                            Content = content
                        });

                        tlg.SendMessage(new Message()
                        {
                            Title = title,
                            Body = content
                        });
                    }
                }
            }
        } catch(Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static Stream Decompress(Stream compressedStream)
    {
        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
        using (var resultStream = new MemoryStream())
        {
            zipStream.CopyTo(resultStream);
            return resultStream;
        }
    }

    private static void ParserWorkerLauncher()
    {
        while (true)
        {
            if (option.KeyChar == '2' || option.KeyChar == '3')
                break;

            List<ParserConfig> parserConfigs = new List<ParserConfig>();
            parserConfigs.Add(new ParserConfig()
            {
                BaseUrl = "https://www.segodnya.ua/allnews.html",
                ContentType = "html",
                ArticleTag = "//article[@class=\"b-article \"]",
                TitleTag = "//div[@class=\"b-article__title\"]/a",
                ContentTag = "//p[@class=\"b-article__content\"]",
            }); 

            foreach(var pc in parserConfigs)
            {
                if (pc.ContentType == "html")
                    Task.Run(() => ParserHtmlWorkerFunc(pc));
                else if (pc.ContentType == "json")
                    Task.Run(() => ParserJsonWorkerFunc(pc));
            }

            Thread.Sleep(config.ParseInterval);
        }
    }

    static void Main(string[] args)
    {
        Thread parserLauncherThread;

        string consoleMessage = "Parser start - 1";
        bool parserStatus = false;
        while (true)
        {
            Console.Clear();
            Console.WriteLine(consoleMessage);
            Console.WriteLine("Exit - 3");
            option = Console.ReadKey();

            if (option.KeyChar == '1' && parserStatus == false)
            {
                parserLauncherThread = new Thread(ParserWorkerLauncher);
                parserLauncherThread.IsBackground = true;
                parserLauncherThread.Start();
                parserStatus = true;
                consoleMessage = "Parser stop - 2";
            }
            else if (option.KeyChar == '2')
            {
                consoleMessage = "Parser start - 1";
                parserStatus = false;
            }

            if (option.KeyChar == '3')
                break;
        }
    }
}

