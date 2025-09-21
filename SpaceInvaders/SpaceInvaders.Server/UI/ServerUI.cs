using SpaceInvaders.Common.Models;
using Spectre.Console;

namespace SpaceInvaders.Server.UI
{
    public abstract class ServerUi
    {
        public static void ShowServerStart(int port)
        {
            AnsiConsole.Write(
                new FigletText("Space Invaders Server")
                    .LeftJustified()
                    .Color(Color.Blue));

            AnsiConsole.MarkupLine($"[green]Server started on port {port}[/]");
        }

        public static void ShowWaitingForPlayers()
        {
            AnsiConsole.Status()
                .Start("Waiting for players...", ctx =>
                {
                    AnsiConsole.MarkupLine("[blue]Listening for incoming connections...[/]");
                    // Using SpinnerKnown.Default as status spinner
                    ctx.Spinner(Spinner.Known.Default);
                    ctx.SpinnerStyle(Style.Parse("green"));
                });
        }

        public static void ShowPlayerRegistered(Player player)
        {
            var table = new Table()
                .AddColumn("New Player Registered")
                .Centered();

            table.AddRow($"[green]Name: {player.Name} {player.Surname}[/]");
            table.AddRow($"[green]Position: ({player.Coordinates.X}, {player.Coordinates.Y})[/]");
            table.AddRow($"[green]Lives: {player.Lives}[/]");

            AnsiConsole.Write(table);
        }

        public static void ShowError(string error)
        {
            AnsiConsole.MarkupLine($"[red]Error: {error}[/]");
        }
    }
}