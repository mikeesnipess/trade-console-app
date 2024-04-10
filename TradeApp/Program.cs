using System.Collections.Concurrent;
using TradeApp.Models;
using TradeApp.Services;

class Program
{
    private const int N = 10000;
    private static readonly Dictionary<string, ConcurrentQueue<TradeData>> tradesPerSymbol = new Dictionary<string, ConcurrentQueue<TradeData>>();

    static void Main(string[] args)
    {
        Console.WriteLine("Enter the symbols separated by comma or space (e.g., btcusdt,ethbtc or btcusdt ethbtc):");
        var input = Console.ReadLine();

        var symbols = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var symbol in symbols)
        {
            tradesPerSymbol[symbol] = new ConcurrentQueue<TradeData>();
            var service = new TradeWebSocketService(symbol, tradesPerSymbol[symbol]);
            var thread = new Thread(service.ConnectAndStartListening);
            thread.Start();
        }

        StartTradeCleanup();
        new Thread(DisplayTrades).Start();
    }

    private static void StartTradeCleanup()
    {
        new Thread(() =>
        {
            while (true)
            {
                foreach (var symbolQueue in tradesPerSymbol.Values)
                {
                    while (symbolQueue.Count > N)
                    {
                        symbolQueue.TryDequeue(out _);
                    }
                }
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        })
        { IsBackground = true }.Start();
    }

    private static void DisplayTrades()
    {
        while (true)
        {
            foreach (var kvp in tradesPerSymbol)
            {
                var symbol = kvp.Key;
                var trades = kvp.Value;

                if (trades.IsEmpty)
                {
                    continue;
                }

                ShowingResult(symbol, trades);
            }
        }
    }

    private static void ShowingResult(string symbol, ConcurrentQueue<TradeData> trades)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{symbol.ToUpper()} new trades:");
        Console.WriteLine("----------------------------------------------------------------");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("|   Price   ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("\t| Quantity ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("| BUY  ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("| SELL ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("|  Timestamp  ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("|");

        while (trades.TryDequeue(out var trade))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"| {trade.Price.ToString("0.00000000")} ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"| {trade.Quantity.ToString("0.00000000")} ");
            if (trade.IsBuyer)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("|  *  ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("|     ");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("|     ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("|  *  ");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("| " + trade.Time.ToString("yyyy-MM-dd HH:mm:ss"));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("|");
        }
        Console.WriteLine("----------------------------------------------------------------");
        Thread.Sleep(1000);

    }
}
