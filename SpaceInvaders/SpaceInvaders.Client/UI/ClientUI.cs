using System;
using SpaceInvaders.Common.Models;
using Spectre.Console;

namespace SpaceInvaders.Client.UI
{
    public abstract class ClientUi
    {
        public static (string name, string surname) GetPlayerInfo()
        {
            AnsiConsole.Write(
                new FigletText("Space Invaders")
                    .LeftJustified()
                    .Color(Color.Green));

            var name = AnsiConsole.Ask<string>("[green]Enter your name:[/]");
            var surname = AnsiConsole.Ask<string>("[green]Enter your surname:[/]");

            return (name, surname);
        }

        public static void ShowConnectionStatus(string status)
        {
            AnsiConsole.MarkupLine($"[blue]{status}[/]");
        }

        public static void ShowPlayerRegistration(Player player)
        {
            var table = new Table()
                .AddColumn("Registration Confirmed")
                .Centered();

            table.AddRow($"[green]Position: ({player.Coordinates.X}, {player.Coordinates.Y})[/]");
            table.AddRow($"[green]Lives: {player.Lives}[/]");
            table.AddRow($"[green]Points: {player.Points}[/]");

            AnsiConsole.Write(table);
        }

        public static void ShowError(string error)
        {
            AnsiConsole.MarkupLine($"[red]Error: {error}[/]");
        }

        public static void WaitForExit()
        {
            AnsiConsole.MarkupLine("[grey]Press any key to exit...[/]");
            Console.ReadKey();
        }
    }
}