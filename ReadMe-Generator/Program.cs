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
using System.Reflection;
using System.Net.Http.Headers;

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
        private static readonly DirectoryInfo templatesDirectory = new DirectoryInfo(Environment.CurrentDirectory + "\\Template\\Install\\");
        private static Template[] installTemplates;
        private static Template installIstruction;
        private static string userName, reposName, appName, logoURL, shortDescription, solutionName, basePath;
        private static bool customURL;


        private static string AskUsername(bool newConfig = false)
        {
            string userName;
            do
            {
                Console.Clear();
                Console.Write("Enter your GitHub username: ");
                userName = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(userName));
            using (StreamWriter savingWriter = new StreamWriter(configFilePath)) savingWriter.WriteLine(userName); //write only first line
            return userName;
        }
        private static string ChooseBase()
        {
            Console.Clear();
            string[] basesPath = Directory.GetFiles(Environment.CurrentDirectory + "\\Template\\");

            if (basesPath.Length < 1)
            {
                WriteWithColor("No base file founded", ConsoleColor.Red, true);
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }

            Console.WriteLine("    Select a base template\n-------------------------------");
            for (int i = 0; i < basesPath.Length; i++) Console.WriteLine($"{i + 1}. {Path.GetFileNameWithoutExtension(basesPath[i])}");
            Console.WriteLine("-------------------------------");

            int reply = -1;
            do
            {
                Console.Write("Choose: ");
                int.TryParse(Console.ReadLine(), out reply);
            } while (reply < 1 || reply > basesPath.Length);

            List<string> settingsContent = File.ReadAllLines(configFilePath).ToList();
            if (settingsContent.Count == 1) settingsContent.Add(basesPath[--reply]);
            else settingsContent[1] = basesPath[--reply];
            File.WriteAllLines(configFilePath, settingsContent);

            return basesPath[reply];
        }
        private static string ParseUserDataHolder(string text) => text.Replace(APP_NAME_HOLDER, appName).Replace(REPOS_HOLDER, reposName).Replace(USER_HOLDER, userName);
        private static int ChooseInstallTemplate(bool mandatoryChoose = false)
        {
            Console.WriteLine("-------------------------------");
            WriteWithColor("0. None", mandatoryChoose == true ? ConsoleColor.DarkGray : Console.ForegroundColor, true);
            for (int i = 0; i < installTemplates.Length; i++) Console.WriteLine($"{i + 1}. {installTemplates[i].Name}");
            while (true)
            {
                Console.Write("Choose: ");
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (mandatoryChoose && --choice > -1 && choice < installTemplates.Length) return choice;
                    else if (!mandatoryChoose && choice > -2 && choice < installTemplates.Length) return choice;
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
            string[] template = File.ReadAllLines(basePath);
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
                    WriteWithColor($"Line {i}. Replacing release link holder => https://github.com/<!-p-!>/<!-r-!>/releases/latest", ConsoleColor.Yellow, true);
                    generatedReadMe.Add(template[i].Replace(RELEASE_LINK_HOLDER, "https://github.com/<!-p-!>/<!-r-!>/releases/latest"));
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
            for (int i = 0; i < generatedReadMe.Count; i++) generatedReadMe[i] = ParseUserDataHolder(generatedReadMe[i]);
            Console.WriteLine("Generating file...");
            string generatedFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "README.md");
            File.WriteAllLines(generatedFilePath, generatedReadMe);
            WriteWithColor($"File successfully generated at {generatedFilePath}", ConsoleColor.Green, true);
            Console.WriteLine("Press a key to continue...");
            Console.ReadKey();
        }
        static void Main()
        {
            if (File.Exists(configFilePath))
            {
                string[] settingContent = File.ReadAllLines(configFilePath);
                if (settingContent.Length != 2)
                {
                    WriteWithColor("Invalid configuration file...", ConsoleColor.Red, true);
                    File.Delete(configFilePath);
                    Console.ReadKey();
                    Environment.Exit(1);
                }
                userName = settingContent[0];
                basePath = settingContent[1];
            }
            else
            {
                userName = AskUsername(true);
                basePath = ChooseBase();
            }

            #region "Checking configuration"
            if (!File.Exists(basePath) || !templatesDirectory.Exists || templatesDirectory.GetFiles("*.itml").Length == 0)
            {
                Console.Clear();
                WriteWithColor("Configuration error", ConsoleColor.DarkRed, true);

                if (File.Exists(basePath)) WriteWithColor($"Base template file exists", ConsoleColor.DarkGreen, true);
                else WriteWithColor($"Base template file doesn't exists", ConsoleColor.Red, true);
                if (templatesDirectory.Exists)
                {
                    Console.Write("Install templates: ");
                    int InstallTemplateFound = templatesDirectory.GetFiles("*.itml").Length;
                    if (InstallTemplateFound > 0) WriteWithColor($"Found {InstallTemplateFound} install templates", ConsoleColor.DarkGreen, true);
                    else WriteWithColor("No install template found", ConsoleColor.Red, true);
                    Console.WriteLine("Press a key to choose another base template...");
                    Console.ReadKey();
                    basePath = ChooseBase();
                    Environment.Exit(1);
                }
                else WriteWithColor("No Install Templates Directory Found", ConsoleColor.Red, true);

                Console.ReadKey();
                Environment.Exit(1);
            }
            #endregion

            FileInfo[] files = templatesDirectory.GetFiles("*.itml");
            installTemplates = new Template[files.Length];
            for (int i = 0; i < files.Length; i++) installTemplates[i] = new Template(files[i]);

            int reply = -1;
            do
            {
                Console.Clear();
                WriteAtCenter("GitHub ReadMe Template Generator");
                Console.Write("Hi ");
                WriteWithColor(userName, ConsoleColor.Yellow);
                Console.WriteLine("!");
                Console.Write("You have selected the ");
                WriteWithColor(Path.GetFileNameWithoutExtension(basePath), ConsoleColor.Cyan);
                Console.WriteLine(" template");
                Console.Write("What do you want to do?\n1. Generate a ReadMe file\n2. Change preferences\n0. Exit\nChoose: ");
                if (int.TryParse(Console.ReadLine(), out reply))
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
                            customURL = Console.ReadKey().Key == ConsoleKey.Y;
                            if (customURL)
                            {
                                Console.Write("\nMake sure that this is a raw URL!\nif using GitHub, for example, check that the link is like this:\nhttps://github.com/$username/$reposname/blob/main/$custompath/Logo.png?raw=true\nInsert your custom URL: ");
                                logoURL = Console.ReadLine();
                            }
                            else logoURL = "https://github.com/<!-p-!>/<!-p-!>/blob/main/Assets/<!-r-!>/Logo.png?raw=true";
                            Console.Write("\rInsert a short description about the project: ");
                            shortDescription = Console.ReadLine();
                            Console.Write("Insert the main solution file name without his extension(Ex. Solution1.sln -> Solution1): ");
                            solutionName = Console.ReadLine();
                            installIstruction = installTemplates[ChooseInstallTemplate(true)];
                            Console.Clear();
                            WriteAtCenter("Report");
                            Console.WriteLine($"\nUser: {userName}\nRepository: {reposName}\nApp Name: {appName}\nLogo link: {(customURL ? "Custom" : "Default")}\nShort Description: {shortDescription}\nSolution file name: {solutionName}\nInstall Setting: {installIstruction.Name}\n\n\nMake sure that these files exist on your GitHub profile:\nLogo link: {ParseUserDataHolder(logoURL)}\nTextLogo: {ParseUserDataHolder("https://github.com/<!-p-!>/<!-p-!>/blob/main/Assets/<!-r-!>/TextLogo.png?raw=true")}\nAppInterface: {ParseUserDataHolder("https://github.com/<!-p-!>/<!-p-!>/blob/main/Assets/<!-r-!>/AppInterface.png?raw=true")}\nDo you wish to continue [Y/n]");
                            if (Console.ReadKey().Key == ConsoleKey.N)
                            {
                                reposName = appName = logoURL = shortDescription = solutionName = string.Empty;
                                installIstruction = null;
                                reply = -1;
                                break;
                            }
                            #endregion
                            WriteWithColor("\n\nStarting generating...", ConsoleColor.Blue, true);
                            GenerateText();
                            break;
                        case 2:
                            #region "Preferences Menu"
                            int choice = -1;
                            do
                            {
                                Console.Clear();
                                WriteAtCenter("Preferences\n");
                                Console.Write("1. Change Username\n2. Change Default Base\n3. Reset all settings\n0. Back\nChoose: ");
                                if (int.TryParse(Console.ReadLine(), out choice))
                                {
                                    switch (choice)
                                    {
                                        case 1: userName = AskUsername(); break;
                                        case 2: basePath = ChooseBase(); break;
                                        case 3:
                                            File.Delete(configFilePath);
                                            Process.Start(Assembly.GetExecutingAssembly().Location);
                                            Environment.Exit(0);
                                            break;
                                    }
                                }
                            } while (choice != 0);
                            break;
                            #endregion
                    }
                }
            } while (reply != 0);
        }
    }
}
