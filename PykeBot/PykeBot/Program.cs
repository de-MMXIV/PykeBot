using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using RiotNet;
using System.Threading.Tasks;

namespace Pyke_Bot
{
    public class Program
    {
        static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        Tokens secretTokens = new Tokens();
        private CommandHandler _handler;

        public async Task StartAsync()
        {
            SqliteConnection connection = await CreateSqliteAsync();

            XpoDefault.DataLayer = XpoDefault.GetDataLayer(connection, AutoCreateOption.DatabaseAndSchema);

            _client = new DiscordSocketClient();

            await _client.LoginAsync(TokenType.Bot, secretTokens.PykeBotToken);

            await _client.StartAsync();

            RiotClient.DefaultSettings = () => new RiotClientSettings
            {
                ApiKey = secretTokens.RiotApiKey
            };

            _handler = new CommandHandler(_client);

            await Task.Delay(-1);
        }

        private async Task<SqliteConnection> CreateSqliteAsync()
        {
            SqliteConnection con = new SqliteConnection("Data Source=database.sqlite;");
            await con.OpenAsync();
            return con;
        }
    }
}