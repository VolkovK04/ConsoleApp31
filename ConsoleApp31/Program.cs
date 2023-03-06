using Discord.Audio;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Discord;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Collections.Concurrent;

internal class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();
    private DiscordSocketClient? _client;
    public async Task MainAsync()
    {
        _client = new DiscordSocketClient();
        _client.Log += Log;
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;

        var token = "MTA4MjEzMzc0MTg2Mzk3Mjg3Nw.GlFmrj.NTTKUtxrdil4n81_Lbnk1GkLV1tRKAliLHpHi8";

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // Block this task until the program is closed.
        await Task.Delay(-1);

    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }



    // The command's Run Mode MUST be set to RunMode.Async, otherwise, being connected to a voice channel will block the gateway thread.
    [Command("join", RunMode = RunMode.Async)]
    public async Task JoinChannel(IVoiceChannel? channel = null)
    {
        // Get the audio channel
        //channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
        //if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

        // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
        if (channel != null)
        {
            audioClient = await channel.ConnectAsync();
        }
    }

    IAudioClient audioClient;
    private async Task SendAsync(IAudioClient client, string path)
    {
        // Create FFmpeg using the previous example
        using (var ffmpeg = CreateStream(path))
        using (var output = ffmpeg.StandardOutput.BaseStream)
        using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
        {
            try { await output.CopyToAsync(discord); }
            finally { await discord.FlushAsync(); }
        }
    }

    public async Task Client_Ready()
    {
        ulong guildId = 1082135098327052310;
        // Let's build a guild command! We're going to need a guild so lets just put that in a variable.
        var guild = _client.GetGuild(guildId);

        // Next, lets create our slash command builder. This is like the embed builder but for slash commands.
        var guildCommand = new SlashCommandBuilder();

        // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
        guildCommand.WithName("first-command");

        // Descriptions can have a max length of 100.
        guildCommand.WithDescription("This is my first guild slash command!");

        var connectToVoiceCommand = new SlashCommandBuilder();
        connectToVoiceCommand.WithName("join");
        connectToVoiceCommand.WithDescription("description");
        var startToVoiceCommand = new SlashCommandBuilder();
        startToVoiceCommand.WithName("play");
        startToVoiceCommand.WithDescription("description");



        try
        {
            // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.
            await guild.CreateApplicationCommandAsync(guildCommand.Build());
            await guild.CreateApplicationCommandAsync(connectToVoiceCommand.Build());
            await guild.CreateApplicationCommandAsync(startToVoiceCommand.Build());

            // With global commands we don't need the guild.

            // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
            // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
        }
        catch (HttpException exception)
        {
            // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

            // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
            Console.WriteLine(json);
        }
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        ulong guildId = 1082135098327052310;
        var guild = _client.GetGuild(guildId);
        await command.RespondAsync($"You executed {command.Data.Name}");
        ulong channelId = 1082135099023310931;
        var channel = guild.GetVoiceChannel(channelId);
        if (command.Data.Name == "join")
        {
            _ = JoinChannel(channel);
        }

        if (command.Data.Name == "play")
        {
            string path = "C:\\Users\\volkov\\Downloads\\sample-3s.wav";
            _ = SendAsync(audioClient, path);
        }
        //SendAudioAsync(guild, channel, );
        //AudioClient audioClient
        //SendAsync(_client.Get, "test.wav");

    }




    private Process CreateStream(string path)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        });
    }
}