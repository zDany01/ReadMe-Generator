using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ReadMe_Generator
{
    internal class Program
    {
        private const string USER_HOLDER = "<!-p-!>";
        private const string REPOS_HOLDER = "<!-r-!>";
        private const string APP_NAME_HOLDER = "<!-n-!>";
        private const string LOGO_HOLDER = "<!-i128-!>";
        private const string SHORT_DESCRIPTION_HOLDER = "<!-sdsc-!>";
        private const string RELEASE_LINK_HOLDER = "<!-releaselink-!>";
        private const string INSTALL_ISTRUCTION_HOLDER = "<!-INSTALLSETTINGS-!>";
        private const string SOLUTION_NAME_HOLDER = "<!-solutionmame-!>";
        private static readonly string configFilePath = Environment.CurrentDirectory + "\\config.ini";
        private static readonly string baseTemplatePath = Environment.CurrentDirectory + "\\Template\\Base.md";
        private static readonly DirectoryInfo InstallTemplateDirectory = new DirectoryInfo(Environment.CurrentDirectory + "\\Template\\Install\\");
        private static string userName, reposName, appName, logoURL;


        private static string SetUsername()
        {
            string userName;
            do
            {
                Console.Clear();
                Console.Write("Enter your GitHub username: ");
                userName = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(userName));
            File.WriteAllText(configFilePath, userName);
            return userName;
        }
        private static void WriteAtCenter(string text)
        {
            Console.SetCursorPosition((Console.WindowWidth - text.Length) / 2, Console.CursorTop);
            Console.WriteLine(text);
        }
        private static void WriteWithColor<T>(T value, ConsoleColor color, bool endl = false)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            if (endl) Console.WriteLine(value);
            else Console.Write(value);
            Console.ForegroundColor = oldColor;
        }
        static void Main()
        {
            userName = File.Exists(configFilePath) ? File.ReadAllLines(configFilePath)[0] : SetUsername();

            if (!File.Exists(baseTemplatePath) || !InstallTemplateDirectory.Exists || InstallTemplateDirectory.GetFiles("*.itml").Length == 0)
            {
                Console.Clear();
                WriteWithColor("Configuration error", ConsoleColor.DarkRed, true);

                if (File.Exists(baseTemplatePath)) WriteWithColor($"Base.md file exists", ConsoleColor.DarkGreen, true);
                else WriteWithColor($"Base.md file doesn't exists", ConsoleColor.Red, true);
                if (InstallTemplateDirectory.Exists)
                {
                    Console.Write("Install templates: ");
                    int InstallTemplateFound = InstallTemplateDirectory.GetFiles("*.itml").Length;
                    if (InstallTemplateFound > 0) WriteWithColor($"Found {InstallTemplateFound} install templates", ConsoleColor.DarkGreen, true);
                    else WriteWithColor("No install template found", ConsoleColor.Red, true);
                }
                else WriteWithColor("No Install Templates Directory Found", ConsoleColor.Red, true);

                Console.ReadKey();
                Environment.Exit(1);
            }


            bool redo;
            do
            {
                redo = false;
                Console.Clear();
                Console.Title = "ReadMe Generator - Main Menu";
                WriteAtCenter("GitHub ReadMe Template Generator");
                Console.Write("Hi ");
                WriteWithColor(userName, ConsoleColor.Yellow);
                Console.WriteLine("!");
                Console.Write("What do you want to do?\n1. Generate a ReadMe file\n2. Change preferences\n0. Exit\nChoose: ");
                if (int.TryParse(Console.ReadLine(), out int reply))
                {
                    switch (reply)
                    {
                        case 0: return;
                        case 1:
                            Console.Title = "ReadMe Generator - Generator";
                            break;
                        case 2:
                            Console.Clear();
                            Console.Title = "ReadMe Generator - Preferences";
                            WriteAtCenter("Preferences\n");
                            Console.Write("1. Change username\n2. Change default install template\n3. Reset all settings\nChoose: ");
                            throw new NotImplementedException();
                            break;
                        default: redo = true; break;
                    }
                }
                else redo = true;
            } while (redo);

            Console.ReadKey();
        }
    }
}
