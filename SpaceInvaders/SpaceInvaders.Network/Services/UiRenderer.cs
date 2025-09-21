using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SpaceInvaders.Common.Game;
using SpaceInvaders.Common.Models;
using Spectre.Console;

namespace SpaceInvaders.Network.Services
{
    public class UiRenderer
    {
        private readonly IAnsiConsole _console;
        private readonly Layout _layout;
        private readonly LiveDisplay _liveDisplay;
        private readonly int _mapHeight;
        private readonly int _mapWidth;
        public GameState GameState;

        public UiRenderer(int height, int width)
        {
            _mapHeight = height;
            _mapWidth = width;
            _console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(Console.Out),
                Interactive = InteractionSupport.No,
                ColorSystem = (ColorSystemSupport)ColorSystem.Standard
            });

            // Create the layout once
            _layout = new Layout("Root")
                .SplitRows(
                    new Layout("Header").Size(6),
                    new Layout("Game")
                );

            // Initialize live display
            _liveDisplay = _console.Live(_layout)
                .AutoClear(false)
                .Overflow(VerticalOverflow.Crop)
                .Cropping(VerticalOverflowCropping.Bottom);

            // Configure console
            Console.CursorVisible = false;
        }

        public void Render()
        {
            if (_liveDisplay == null)
                return;

            // Wait for game to start
            while (GameState == null || !GameState.IsGameStarted) Thread.Sleep(100);

            _liveDisplay.StartAsync(ctx =>
            {
                while (GameState.IsGameStarted)
                {
                    // Create header
                    var headerTable = new Table()
                        .Border(TableBorder.Rounded)
                        .BorderColor(Color.Blue)
                        .AddColumn(new TableColumn("[blue]Players Status[/]").Centered());

                    var playerGrid = new Grid()
                        .AddColumn()
                        .AddColumn();

                    foreach (var player in GameState.Players)
                    {
                        var livesDisplay = string.Join("", Enumerable.Repeat("♥", player.Lives));
                        playerGrid.AddRow(
                            $"[blue]Player {player.Id}:[/] {player.Name} {player.Surname}",
                            $"[red]{livesDisplay}[/] [yellow]Points:[/] {player.Points}"
                        );
                    }

                    headerTable.AddRow(playerGrid);
                    _layout["Header"].Update(headerTable);

                    // Create game grid
                    var gameGrid = new Table()
                        .Border(TableBorder.None)
                        .HideHeaders()
                        .Centered()
                        .Expand();

                    // Add columns
                    for (var i = 0; i < _mapWidth; i++)
                        gameGrid.AddColumn(new TableColumn("    ").Width(4).Centered());

                    // Create the game field
                    var field = new string[_mapHeight, _mapWidth];
                    for (var y = 0; y < _mapHeight; y++)
                    for (var x = 0; x < _mapWidth; x++)
                        field[y, x] = " ";

                    // Draw game elements
                    foreach (var obstacle in GameState.Obstacles.Where(obstacle => IsInBounds(obstacle.Coordinates)))
                        field[obstacle.Coordinates.Y, obstacle.Coordinates.X] =
                            $"[yellow]{obstacle.GetDisplaySymbol()}[/]";

                    foreach (var projectile in GameState.Projectiles.Where(projectile =>
                                 IsInBounds(projectile.Coordinates)))
                        field[projectile.Coordinates.Y, projectile.Coordinates.X] =
                            "[red]^[/]";

                    foreach (var player in GameState.Players.Where(player => IsInBounds(player.Coordinates)))
                        field[player.Coordinates.Y, player.Coordinates.X] =
                            $"[blue]A{player.Id}[/]";

                    // Add rows to the game grid
                    for (var y = 0; y < _mapHeight; y++)
                    {
                        var rowCells = new string[_mapWidth];
                        for (var x = 0; x < _mapWidth; x++)
                            if (string.IsNullOrEmpty(field[y, x]))
                                rowCells[x] = "    ";
                            else
                                rowCells[x] = field[y, x];
                        gameGrid.AddRow(rowCells);
                    }

                    var gamePanel = new Panel(gameGrid)
                        .BorderColor(Color.DarkBlue)
                        .Border(BoxBorder.Rounded)
                        .BorderStyle(new Style(Color.DarkBlue));

                    _layout["Game"].Update(gamePanel);

                    // Render the entire layout
                    ctx.Refresh();
                    Thread.Sleep(25);
                }

                return Task.CompletedTask;
            });
        }

        private bool IsInBounds(Coordinates coords)
        {
            return coords.X >= 0 && coords.X < _mapWidth &&
                   coords.Y >= 0 && coords.Y < _mapHeight;
        }
    }
}