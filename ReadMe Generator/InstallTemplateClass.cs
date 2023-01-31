using System.IO;

namespace ReadMe_Generator
{
    internal class InstallTemplateClass
    {
        public string Name { get; private set; }
        public string Text { get; private set; }
        public InstallTemplateClass(FileInfo templateFile)
        {
            string[] fileContent = File.ReadAllLines(templateFile.FullName);
            Name = fileContent[0];
            Text = string.Empty;
            for (int i = 2; i < fileContent.Length; i++) Text += fileContent[i] + '\n';
        }
    }
}
