using System;
using System.Runtime.InteropServices;
using System.Text;
using SpaceInvaders.Client.UI;
using Spectre.Console;

namespace SpaceInvaders.Client
{
    internal class Program
    {
        // Prevent window resize
        private const int MfBycommand = 0x00000000;
        private const int ScSize = 0xF000;
        private const int ScMinimize = 0xF020;
        private const int ScMaximize = 0xF030;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Set console size
            Console.WindowWidth = 175;
            Console.WindowHeight = 40;

            // Disable window resizing
            var handle = GetConsoleWindow();
            var sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, ScSize, MfBycommand);
                DeleteMenu(sysMenu, ScMinimize, MfBycommand);
                DeleteMenu(sysMenu, ScMaximize, MfBycommand);
            }

            var client = new GameClient();
            try
            {
                client.Start();
            }
            catch (Exception ex)
            {
                AnsiConsole.Clear();
                ClientUi.ShowError(ex.Message);
            }
        }
    }
}