using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

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
        private const string SOLUTION_NAME_HOLDER = "<!-solutionmame-!>";
        private const string INSTALL_ISTRUCTION_HOLDER = "<!-INSTALLSETTINGS-!>";
        private static readonly string configFilePath = Environment.CurrentDirectory + "\\config.ini";
        private static readonly string baseTemplatePath = Environment.CurrentDirectory + "\\Template\\Base.md";
        private static readonly DirectoryInfo InstallTemplateDirectory = new DirectoryInfo(Environment.CurrentDirectory + "\\Template\\Install\\");
        private static InstallTemplateClass[] installTemplateFiles;
        private static string userName, reposName, appName, logoURL, shortDescription, releaseLink, installIstruction, solutionName;


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
        private static string ParseHolder(string value) => value.Replace(INSTALL_ISTRUCTION_HOLDER, installIstruction).Replace(SOLUTION_NAME_HOLDER, solutionName).Replace(RELEASE_LINK_HOLDER, releaseLink).Replace(SHORT_DESCRIPTION_HOLDER, shortDescription).Replace(LOGO_HOLDER, logoURL).Replace(APP_NAME_HOLDER, appName).Replace(REPOS_HOLDER, reposName).Replace(USER_HOLDER, userName);
        private static int ChooseInstallTemplate(bool mandatoryChoose = false)
        {
            Console.WriteLine("-------------------------------");
            WriteWithColor("0. None", mandatoryChoose == true ? ConsoleColor.Gray : Console.ForegroundColor, true);
            for (int i = 0; i < installTemplateFiles.Length; i++) Console.WriteLine($"{i + 1}. {installTemplateFiles[i].Name}");
            while(true)
            {
                Console.Write("Choose: ");
                if(int.TryParse(Console.ReadLine(), out int choice))
                {
                    choice--;
                    if (mandatoryChoose && choice > -1 && choice < installTemplateFiles.Length) return choice;
                    else if (!mandatoryChoose && choice > -2 && choice < installTemplateFiles.Length) return choice;
                }
            }
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
            else
            {
                FileInfo[] files = InstallTemplateDirectory.GetFiles("*.itml");
                installTemplateFiles = new InstallTemplateClass[files.Length];
                for (int i = 0; i < files.Length; i++) installTemplateFiles[i] = new InstallTemplateClass(files[i]);
            }

            bool redo;
            do
            {
                redo = false;
                Console.Clear();
                WriteAtCenter("GitHub ReadMe Template Generator");
                Console.Write("Hi ");
                WriteWithColor(userName, ConsoleColor.Yellow);
                Console.WriteLine("!");
                Console.Write("What do you want to do?\n1. Generate a ReadMe file\n2. Change preferences\n0. Exit\nChoose: ");
                if (int.TryParse(Console.ReadLine(), out int reply))
                {
                    switch (reply)
                    {
                        case 1:
                            Console.Write("Insert your repository name: ");
                            reposName = Console.ReadLine();
                            Console.Write("Insert your Application name: ");
                            appName = Console.ReadLine();
                            Console.Write("Do you want to use a custom logo URL? [y/N]: ");
                            if (Console.ReadKey().Key == ConsoleKey.Y)
                            {
                                Console.Write("Make sure that this is a raw URL!\nif using GitHub, for example, check that the link is like this:\nhttps://github.com/$username/$reposname/blob/main/$custompath/Logo.png?raw=true\nInsert your custom URL: ");
                                logoURL = $"<img src=\"{Console.ReadLine()}\" alt=\"Logo\" width=\"80\" height=\"80\">";
                            }
                            else logoURL = ParseHolder("<img src=\"https://github.com/<!-p-!>/<!-p-!>/blob/main/Assets/<!-r-!>/Logo.png?raw=true\" alt=\"Logo\" width=\"80\" height=\"80\">");
                            Console.Write("Insert a short description about the project: ");
                            shortDescription = Console.ReadLine();
                            Console.Write("Insert the latest prebuilt binary release direct link: ");
                            releaseLink = Console.ReadLine();
                            Console.Write("Insert the main solution file name without his extension(Ex. Solution1.sln -> Solution1): ");
                            solutionName = Console.ReadLine();
                            installIstruction = installTemplateFiles[ChooseInstallTemplate(true)].Text;
                            break;
                        case 2:
                            Console.Clear();
                            WriteAtCenter("Preferences\n");
                            Console.Write("1. Change username\n2. Change default install template\n3. Reset all settings\nChoose: ");
                            throw new NotImplementedException();
                            break;
                        case 0: return;
                        default: redo = true; break;
                    }
                }
                else redo = true;
            } while (redo);

            Console.ReadKey();
        }
    }
}
