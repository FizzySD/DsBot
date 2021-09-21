using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Victoria;

namespace Victoria.Entities
{
    public class MusicService
    {
        private LavaSocketClient _lavaSocketClient;
        private LavaRestClient _lavaRestClient;
        private DiscordSocketClient _client;
        private LavaPlayer player;
        public MusicService(LavaRestClient lavaRestClient, LavaSocketClient lavaSocketClient, DiscordSocketClient client)
        {
            _client = client;
            _lavaRestClient = lavaRestClient;
            _lavaSocketClient = lavaSocketClient;
        }
        
        public Task initializeAsync() 
        {
            _client.Ready += ClientReadyAsync;
            _lavaSocketClient.Log += LogAsync;
            _lavaSocketClient.OnTrackFinished += OnTrackFinished;
            return Task.CompletedTask;
        }

        public async Task ConnectAsync(SocketVoiceChannel Voicechannel, ITextChannel Textchannel)
            => await _lavaSocketClient.ConnectAsync(Voicechannel, Textchannel);

        public async Task LeaveAsync(SocketVoiceChannel Voichechannel)
            => await _lavaSocketClient.DisconnectAsync(Voichechannel);

        public async Task<string> PlaySong(String query, ulong guildId) 
        {
            player = _lavaSocketClient.GetPlayer(guildId);
            var resoult = await _lavaRestClient.SearchYouTubeAsync(query);
            if (resoult.LoadType == LoadType.NoMatches || resoult.LoadType == LoadType.LoadFailed) 
            {
                return "Ho cercato anche dal mercato dei lipilli, ma non ho trovato nula nula";
            }
            var track = resoult.Tracks.FirstOrDefault();

            if (player.IsPlaying)
            {
                player.Queue.Enqueue(track);
                return $"{track.Title} è stata aggiunta alle canzoni da far cantare allo schiavetto, ora sculacciami puttana";
            }
            else 
            {
                await player.PlayAsync(track);
                return $"{track.Title} sta venendo cantata dalla vostra brava donna";
            }
        }
        private async Task ClientReadyAsync()
        {
            await _lavaSocketClient.StartAsync(_client,new Configuration 
            {
                LogSeverity = LogSeverity.Info
            });
        }

        private async Task OnTrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason) 
        {
            if (reason.ShouldPlayNext()) 
            { 
                return;
            }
            if (!player.Queue.TryDequeue(out var item) || !(item is LavaTrack nextTrack)) 
            {
                await player.TextChannel.SendMessageAsync("Non ci sono canzoni in riproduzione");
                return;
            }
            await player.PlayAsync(nextTrack);
        } 
        private Task LogAsync(LogMessage console)
        {
            Console.WriteLine(console.Message);
            return Task.CompletedTask;
        }
    

    }
}
