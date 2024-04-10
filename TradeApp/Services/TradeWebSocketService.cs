using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using TradeApp.Models;

namespace TradeApp.Services
{
    public class TradeWebSocketService
    {
        private readonly ClientWebSocket _client;
        private readonly ConcurrentQueue<TradeData> _trades;
        private readonly string _symbol;

        public TradeWebSocketService(string symbol, ConcurrentQueue<TradeData> trades)
        {
            _symbol = symbol;
            _trades = trades;
            _client = new ClientWebSocket();
        }

        public void ConnectAndStartListening()
        {
            try
            {
                var uri = new Uri($"wss://stream.binance.com:9443/ws/{_symbol.ToLower()}@trade");
                _client.ConnectAsync(uri, CancellationToken.None).Wait();
                StartReceiving();
            }
            catch (Exception ex)
            {
                Console.WriteLine("WebSocket connection error: " + ex.Message);
            }
        }

        private void StartReceiving()
        {
            var buffer = new byte[1024 * 4];
            while (_client.State == WebSocketState.Open)
            {
                var result = _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result; 
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string jsonString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ProcessMessage(jsonString);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
            }
        }

        private void ProcessMessage(string message)
        {
            try
            {
                var tradeData = JsonConvert.DeserializeObject<TradeData>(message);
                if (tradeData != null)
                {
                    _trades.Enqueue(tradeData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }
    }
}
