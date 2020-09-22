# YetAnotherBot
OwO (The extremely popular osu! bot) Rewritten and copied from scratch in C#. Why? because owo has way too many restrictions and doesnt work 30% of the time, enough for it to be extremely annoying so i made my own. Yeah im venting lol, also code is extremely spaghetti so i'm gonna tidy it up some time. Okay so i dont know how to use github, but the bot works as plugin based, the Bot knows about BotPluginBase, but basically the plugin itself doesn't know about the bot, The osu! plugin needs to have https://github.com/Francesco149/oppai-ng to be in the base directory (where the bot exe is) for it to work, also don't forget to put your osu v1 api key in Credentials.cs and discord bot api key in the other Credentials.cs

<h1>Features<h1>
I haven't implemented tracking and idk if i will 
also this bot has chinese knockoff vibes

">rs <user>" || ">rs" || ">rs -l", -l means show as many recent of your or someones elses plays as discord allows
">top" || "top -r", -r means show your most recent top plays, both commands returns as many plays as discord allows
">flex <indexTop>" // >c but for >top
">c" || ">c <user>"//compare your scores or someone elses scores with the most recent play in the channel
">match <user>" //Match your osu profile with someone elses
">osu" //shows your osu profile
">set <username>" //make it so the bot knows what your osu username is
