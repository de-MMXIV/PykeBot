using DevExpress.Xpo;
using Discord;
using Discord.Commands;
using Pyke_Bot.DataModel;
using RiotNet;
using RiotNet.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PykeBot;

namespace Pyke_Bot.Modules
{
    public class Command : ModuleBase<SocketCommandContext>
    {
        private readonly IRiotClient _client = new RiotClient();
        RiotFunctions riotFunctions = new RiotFunctions();


        [Command("register")]
        [Alias("reg")]
        private async Task Register(string region, [Remainder] string name)
        {
            EmbedBuilder embedBuilder;
            try
            {
                Summoner summoner = await _client.GetSummonerBySummonerNameAsync(name, riotFunctions.MatchServers(region));
                string summonerId = summoner.Id;
                using (UnitOfWork uow = new UnitOfWork())
                {
                    IQueryable<UserReference> idCheck = uow.Query<UserReference>().Where(reference => reference.DiscordId == Context.User.Id);
                    UserReference newInfo = idCheck.Count() == 1 ? idCheck.First() : new UserReference(uow);

                    newInfo.DiscordId = Context.User.Id;
                    newInfo.RiotId = summonerId;
                    newInfo.Region = region;
                    newInfo.Date = DateTime.Now;
                    uow.CommitChanges();
                }
                embedBuilder = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle("Success")
                    .WithDescription($"{Context.User.Mention} is now linked to {name}");
            }
            catch (NullReferenceException e)
            {
                embedBuilder = new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithTitle("Failed")
                    .WithDescription($"{name} is not a valid Username in the declared Region");
            }

            await Context.Channel.SendMessageAsync(null, false, embedBuilder.Build(), null);
        }



        [Command("qs")]
        [Alias("queueStats")]
        private async Task QStats()
        {
            EmbedBuilder embed = new EmbedBuilder();
            float wins = 0;
            float losses = 0;

            using (UnitOfWork uow = new UnitOfWork())
            {
                IQueryable<UserReference> idCheck = uow.Query<UserReference>().Where(reference => reference.DiscordId == Context.User.Id);
                if (idCheck.Count() == 1)
                {
                    float totalGames = 0;
                    float winPercent = 0;
                    float qWins = 0;
                    float qLosses = 0;
                    UserReference user = idCheck.First();

                    System.Collections.Generic.List<LeagueEntry> leagueEntries = await _client.GetLeagueEntriesBySummonerIdAsync(user.RiotId, riotFunctions.MatchServers(user.RiotId));

                    if (leagueEntries.Count == 0)
                    {
                        embed.WithColor(Color.Red)
                            .WithTitle("Failed")
                            .WithDescription("There was a Error while fetching your Profile Data \n" +
                                             "This might occure because of Account inactivity");
                    }

                    foreach (LeagueEntry league in leagueEntries)
                    {
                        wins += league.Wins;
                        losses += league.Losses;
                        if (league.QueueType == "RANKED_SOLO_5x5")
                        {
                            qLosses = league.Losses;
                            qWins = league.Wins;
                            embed.AddField("Ranked Solo", $"Rank: {league.Tier} {league.Rank}\n" +
                                                          $"League Points: {league.LeaguePoints}\n" +
                                                          $"Wins: {league.Wins}\n" +
                                                          $"Losses: {league.Losses}\n" +
                                                          $"Win Percent: {Math.Round((qWins / (qWins + qLosses)) * 100)}%", true);
                        }
                        if (league.QueueType == "RANKED_FLEX_SR")
                        {
                            qLosses = league.Losses;
                            qWins = league.Wins;
                            embed.AddField("Ranked Flex", $"Rank: {league.Tier} {league.Rank}\n" +
                                                          $"League Points: {league.LeaguePoints}\n" +
                                                          $"Wins: {league.Wins}\n" +
                                                          $"Losses: {league.Losses}\n" +
                                                          $"Win Percent: {Math.Round((qWins / (qWins + qLosses)) * 100)}%", true);
                        }
                    }

                    totalGames = losses + wins;
                    winPercent = (wins / totalGames) * 100;

                    if (totalGames >= 1)
                    {
                        embed.AddField("General Stats", $"Total Games: {totalGames}\n" +
                                                        $"Total Wins: {wins}\n" +
                                                        $"Total Losses: {losses}\n" +
                                                        $"Total Win Percent: {Math.Round(winPercent, 2)}%")
                                                        .WithColor(Color.Blue);
                    }

                    user.Date = DateTime.Now;
                    uow.CommitChanges();
                }
                else
                {
                    embed.WithColor(Color.Red)
                        .WithTitle("Failed")
                        .WithDescription("You are not linked to a League of Legends account!\n" +
                                         "We can't search for your stats if we don't know you :^)\n" +
                                         "If you don't know how to link your account use .help");
                }
                await Context.Channel.SendMessageAsync(null, false, embed.Build());

            }
        }



        [Command("cs")]
        [Alias("championStats")]
        private async Task CStats()
        {
            EmbedBuilder embed = new EmbedBuilder();
            using (UnitOfWork uow = new UnitOfWork())
            {
                IQueryable<UserReference> idCheck = uow.Query<UserReference>()
                    .Where(reference => reference.DiscordId == Context.User.Id);
                if (idCheck.Count() == 1)
                {
                    UserReference user = idCheck.First();
                    var championMasteries = await _client.GetChampionMasteriesAsync(user.RiotId, riotFunctions.MatchServers(user.Region));
                    for (int i = 0; i < 5; i++)
                    {
                        embed
                            .AddField($"Champion: {riotFunctions.GetChampions(championMasteries[i].ChampionId)}",
                                $"Champion Level: {championMasteries[i].ChampionLevel}\n" +
                                $"Champion Points: {championMasteries[i].ChampionPoints}")
                            .WithColor(Color.Blue);
                    }
                }
            }
            await Context.Channel.SendMessageAsync(null, false, embed.Build());
        }



        [Command("deleteProfile")]
        private async Task DeleteProfile()
        {
            using (UnitOfWork uow = new UnitOfWork())
            {
                IQueryable<UserReference> idCheck = uow.Query<UserReference>().Where(reference => reference.DiscordId == Context.User.Id);
                if (idCheck.Count() == 1)
                {
                    UserReference user = idCheck.First();
                    uow.Delete(user);
                    uow.CommitChanges();
                }
            }
        }

    }
}
