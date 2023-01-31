using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Globalization;

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
        private static InstallTemplateClass[] installTemplateFiles;
        private static InstallTemplateClass installIstruction;
        private static string userName, reposName, appName, logoURL, shortDescription, releaseLink, solutionName;


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
        private static string ParseUserDataHolder(string text) => text.Replace(APP_NAME_HOLDER, appName).Replace(REPOS_HOLDER, reposName).Replace(USER_HOLDER, userName);
        private static int ChooseInstallTemplate(bool mandatoryChoose = false)
        {
            Console.WriteLine("-------------------------------");
            WriteWithColor("0. None", mandatoryChoose == true ? ConsoleColor.DarkGray : Console.ForegroundColor, true);
            for (int i = 0; i < installTemplateFiles.Length; i++) Console.WriteLine($"{i + 1}. {installTemplateFiles[i].Name}");
            while (true)
            {
                Console.Write("Choose: ");
                if (int.TryParse(Console.ReadLine(), out int choice))
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
        private static void GenerateText()
        {
            Console.Write("Reading all lines from base...");
            string[] template = File.ReadAllLines(baseTemplatePath);
            List<string> generatedReadMe = new List<string>();
            WriteWithColor("\rDone                          ", ConsoleColor.Green, true);
            Console.WriteLine("Analyzing and replacing every line...");
            for (int i = 0; i < template.Length; i++)
            {
                if (template[i].Contains(LOGO_HOLDER))
                {
                    WriteWithColor($"Line {i}. Replacing logo holder => {logoURL}", ConsoleColor.Yellow, true);
                    generatedReadMe.Add(template[i].Replace(LOGO_HOLDER, logoURL));
                }
                else if (template[i].Contains(SHORT_DESCRIPTION_HOLDER))
                {
                    WriteWithColor($"Line {i}. Replacing short description holder => {shortDescription}", ConsoleColor.Yellow, true);
                    generatedReadMe.Add(template[i].Replace(SHORT_DESCRIPTION_HOLDER, shortDescription));
                }
                else if (template[i].Contains(RELEASE_LINK_HOLDER))
                {
                    WriteWithColor($"Line {i}. Replacing release link holder => {releaseLink}", ConsoleColor.Yellow, true);
                    generatedReadMe.Add(template[i].Replace(RELEASE_LINK_HOLDER, releaseLink));
                }
                else if (template[i].Contains(INSTALL_ISTRUCTION_HOLDER))
                {
                    WriteWithColor($"Line {i}. Replacing install instruction holder => {installIstruction.Name}", ConsoleColor.Yellow, true);
                    generatedReadMe.Add(template[i].Replace(INSTALL_ISTRUCTION_HOLDER, installIstruction.Text.Replace(SOLUTION_NAME_HOLDER, solutionName)));
                }
                else
                {
                    Console.WriteLine($"Line {i}. {template[i]}");
                    generatedReadMe.Add(template[i]);
                }
            }
            WriteWithColor("ReadMe successfully generated!", ConsoleColor.Green, true);
            WriteWithColor("Parsing user data holders...", ConsoleColor.Yellow, true);
            for(int i = 0; i < generatedReadMe.Count; i++) generatedReadMe[i] = ParseUserDataHolder(generatedReadMe[i]);
            Console.WriteLine("Generating file...");
            string generatedFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "README.md");
            File.WriteAllLines(generatedFilePath, generatedReadMe);
            WriteWithColor($"File successfully generated at {generatedFilePath}", ConsoleColor.Green, true);
            Console.WriteLine("Press a key to continue...");
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
                            #region "Information Gathering"
                            Console.Write("Insert your repository name: ");
                            reposName = Console.ReadLine();
                            Console.Write("Insert your Application name: ");
                            appName = Console.ReadLine();
                            Console.Write("Do you want to use a custom logo URL? [y/N]: ");
                            if (Console.ReadKey().Key == ConsoleKey.Y)
                            {
                                Console.Write("\nMake sure that this is a raw URL!\nif using GitHub, for example, check that the link is like this:\nhttps://github.com/$username/$reposname/blob/main/$custompath/Logo.png?raw=true\nInsert your custom URL: ");
                                logoURL = Console.ReadLine();
                            }
                            else logoURL = "https://github.com/<!-p-!>/<!-p-!>/blob/main/Assets/<!-r-!>/Logo.png?raw=true";
                            Console.Write("\rInsert a short description about the project: ");
                            shortDescription = Console.ReadLine();
                            Console.Write("Insert the latest prebuilt binary release direct link: ");
                            releaseLink = Console.ReadLine();
                            Console.Write("Insert the main solution file name without his extension(Ex. Solution1.sln -> Solution1): ");
                            solutionName = Console.ReadLine();
                            installIstruction = installTemplateFiles[ChooseInstallTemplate(true)];
                            Console.Clear();
                            WriteAtCenter("Report");
                            Console.WriteLine($"\nUser: {userName}\nRepository: {reposName}\nApp Name: {appName}\nLogo link: {ParseUserDataHolder(logoURL)}\nShort Description: {shortDescription}\nRelease link: {releaseLink}\nSolution file name: {solutionName}\nInstall Setting: {installIstruction.Name}\n\n\nMake sure that these files exist on your GitHub profile: Logo.png, TextLogo.png, AppInterface.png\nDo you wish to continue [Y/n]");
                            if (Console.ReadKey().Key == ConsoleKey.N)
                            {
                                reposName = appName = logoURL = shortDescription = releaseLink = solutionName = string.Empty;
                                installIstruction = null;
                                redo = true;
                                break;
                            }
                            #endregion
                            WriteWithColor("\n\nStarting generating...", ConsoleColor.Blue, true);
                            GenerateText();
                            redo = true;
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
