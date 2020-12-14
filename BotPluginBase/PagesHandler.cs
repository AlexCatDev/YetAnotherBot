using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotPluginBase
{
    public static class PagesHandler
    {
        private static Dictionary<ulong, Pages> pagesDict = new Dictionary<ulong, Pages>();

        public static void SendPages(ISocketMessageChannel msgChannel, Pages pages)
        {
            var sendMessage = msgChannel.SendMessageAsync("", false, pages.GetFirst.Build()).Result;

            if (pages.PageCount > 1)
            {
                sendMessage.AddReactionsAsync(new IEmote[] { new LeftArrowEmote(), new RightArrowEmote() }).GetAwaiter().GetResult();

                pagesDict.Add(sendMessage.Id, pages);
            }
        }

        public static void Handle(IUserMessage msg, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot)
                return;

            Pages page;
            if (pagesDict.TryGetValue(msg.Id, out page))
            {
                if (reaction.Emote.Name == "➡")
                    page.Handle(msg, PageDirection.Forwards);
                else if (reaction.Emote.Name == "⬅")
                    page.Handle(msg, PageDirection.Backwards);
                else
                    return;

                msg.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            }
        }
    }
}
