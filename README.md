## TidalDiscord
Sets your playing status on Discord for TIDAL player.

### About TidalDiscord
**TidalDiscord** is a WinForm .NET application that sets the current track playing on TIDAL as a Rich Presence status on Discord.

No affiliation with TIDAL or Discord.

[TIDAL](https://tidal.com/) is a trademark of Aspiro AB.
[Discord](https://discord.com/) is a trademark of Discord Inc.

### OS Support

It was tested on:

* Windows 10 x64
* Windows 11 x64

### How to use

Simply download the required installer from the [releases](https://github.com/ricardag/tidal-discord/releases) page. There are two installers, one (smaller) for computers with .NET Desktop runtime already installed, and another (larger) for a self-contained executable, that don't requires .NET Desktop presence.

### How to build

I've used **Microsoft Visual Studio 2022** to develop and build the application. There is a VS solution (TidalDiscord.sln) and a project (DiscordTidal/TidalDiscord.csproj) on the source code.

**TidalDiscord** uses .NET Desktop Runtime 6+. If required, [download](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime) and install it directly from Microsoft.

The installer is build using [NSIS](https://nsis.sourceforge.io/Main_Page) with [NsProcess](https://nsis.sourceforge.io/NsProcess_plugin) plugin.

### How to use TidalDiscord

Once run, **TidalDiscord** runs hidden on the desktop, and sets a blue Tidal icon on the taskbar tray. Clicking on it opens the app main window, which display TIDAL player status and current playing track, and also Discord client status and current user.

It keeps automatically monitoring presence of both TIDAL player and Discord client.

### External libs and special thanks

* [DarkModeUI](https://github.com/ricardodalarme/DarkUI)
* [DiscordRichPresence](https://github.com/Lachee/discord-rpc-csharp)


