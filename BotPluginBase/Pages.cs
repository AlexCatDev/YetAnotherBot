using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotPluginBase
{
    public enum PageDirection
    {
        Forwards,
        Backwards
    }

    public class LeftArrowEmote : IEmote
    {
        public string Name => "⬅";
    }

    public class RightArrowEmote : IEmote
    {
        public string Name => "➡";
    }

    public class Pages
    {
        List<EmbedBuilder> pages = new List<EmbedBuilder>();

        public int PageCount => pages.Count;

        private int pageIndex = 0;

        public Pages()
        {

        }

        public void AddContent(EmbedBuilder eb)
        {
            pages.Add(eb);
        }

        public EmbedBuilder GetFirst => pages[0].WithFooter($"Page: {pageIndex + 1}/{pages.Count}");

        public void Handle(IUserMessage msg, PageDirection pageDirection)
        {
            if (pageDirection == PageDirection.Forwards)
                pageIndex = Extensions.Clamp(pageIndex + 1, 0, pages.Count - 1);
            else
                pageIndex = Extensions.Clamp(pageIndex - 1, 0, pages.Count - 1);

            var pageToDisplay = pages[pageIndex];

            pageToDisplay.WithFooter($"Page: {pageIndex + 1}/{pages.Count}");

            msg.ModifyAsync(a => a.Embed = pages[pageIndex].Build());
        }
    }
}
