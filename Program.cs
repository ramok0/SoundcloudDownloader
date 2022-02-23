using PuppeteerSharp;
using System.Diagnostics;
using System.Net;
using System.Text;

class Program
{
    public static List<byte[]> files = new List<byte[]>();
    public static List<string> linksList = new List<string>();
    public static string Title { get; set; }
    private static void Main()
    {
        Start();
        System.Threading.Thread.Sleep(-1);
    }

    private static async void Start()
    {
        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        await using var browser = await Puppeteer.LaunchAsync(
    new LaunchOptions { Headless = true });
        await using var page = await browser.NewPageAsync();
        page.Response += Page_Response;
        Console.WriteLine("Link : ");
        string link = Console.ReadLine();
        await page.GoToAsync(link);
        Title = await page.EvaluateExpressionAsync<string>("document.querySelector(\".soundTitle__titleHeroContainer > h1 > span\").innerText");
        System.Threading.Thread.Sleep(15000);
        Console.WriteLine($"Got {linksList.Count} links");
        linksList.ForEach(link =>
        {
            var client = new WebClient();
            var data = client.DownloadData(link);
            files.Add(data);
        });

        byte[] result = new byte[files.Sum(a => a.Length)];
        using(var stream = new MemoryStream(result))
        {
            foreach (var bytes in files)
                stream.Write(bytes, 0, bytes.Length);
        }

        File.WriteAllBytes(Title + ".mp3", result);
        browser.CloseAsync();
        Environment.Exit(9);
    }

    private static async void Page_Response(object? sender, ResponseCreatedEventArgs e)
    {
        //Console.WriteLine(e.Response.Url);
        if(e.Response.Url.Contains("playlist.m3u8"))
        {
            var buffer = await e.Response.BufferAsync();
            var stringContent = Encoding.ASCII.GetString(buffer).Split("\n");
            var links = stringContent.Where(line => line.StartsWith("https://")).ToList();
            linksList.AddRange(links);
   /*         links.ForEach(async(link) =>
            {
                var client = new WebClient();
                var content = client.DownloadData(link);
                files.Add(content);
            });

            byte[] result = new byte[files.Sum(a => a.Length)];
            using (var stream = new MemoryStream(result))
            {
                foreach (byte[] bytes in files)
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            File.WriteAllBytes(Title + ".mp3", result);
            Console.WriteLine("Finished !");*/
        }
    }
}