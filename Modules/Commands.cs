using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Victoria;
using Victoria.Entities;

namespace TutorialBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private MusicService _musicService;

        public Commands(MusicService musicService) 
        {
            _musicService = musicService;
        }

        [Command("clear", RunMode = RunMode.Async)]
        [Summary("Downloads and removes X messages from the current channel.")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task CancellaMessaggi(int amount)
        {
            // Controlla se il numero dei messaggi è positivo e non nullo
            if (amount <= 0 || amount == null)
            {
                await ReplyAsync("Devi specificare un numero di messaggi");
                return;
            }

            var msg = await Context.Channel.GetMessageAsync(Context.Message.Id);
            var Messaggi =  await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();

            var MessaggiFiltrati = Messaggi.Where(oldmessage => (DateTimeOffset.UtcNow - oldmessage.Timestamp).TotalDays <= 14);
               
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(MessaggiFiltrati);
            await (Context.Channel as ITextChannel).DeleteMessageAsync(msg);
        }
        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinVC() 
        {
            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null) 
            {
                await ReplyAsync("Non sei un una chat vocale coglione");
                return;
            }
            else 
            {
                await _musicService.ConnectAsync(user.VoiceChannel,Context.Channel as ITextChannel);
                await ReplyAsync($"Mi sono aggregato come na catapulta a {user.VoiceChannel.Name}");
            }

        }

        [Command("leave", RunMode = RunMode.Async)]
        public async Task QuitVC()
        {
            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null) 
            {
                await ReplyAsync("Manco sono entrato e già mi chiedi di andarmene.. :broken_heart:");
                return;
            }
            await ReplyAsync("vaffanculo bastardo mi hai rovinato la voce");
            await _musicService.LeaveAsync(user.VoiceChannel);
            
        }
        [Command("Play", RunMode = RunMode.Async)]
        public async Task Play([Remainder] string searchQuery) 
        {

            var resoult = await _musicService.PlaySong(searchQuery, Context.Guild.Id);
            await ReplyAsync(resoult);
        }
    }
}
