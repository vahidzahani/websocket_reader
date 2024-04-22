using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace websocket_reader
{
    public class IniFile
    {
        private readonly string filePath;
        private readonly Dictionary<string, Dictionary<string, string>> sections;

        public IniFile(string filePath)
        {
            this.filePath = filePath;
            this.sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            Load();
        }

        private void Load()
        {
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                string currentSection = "";

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();

                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                        sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                    else
                    {
                        int equalsIndex = trimmedLine.IndexOf('=');

                        if (equalsIndex > 0)
                        {
                            string key = trimmedLine.Substring(0, equalsIndex).Trim();
                            string value = trimmedLine.Substring(equalsIndex + 1).Trim();
                            sections[currentSection][key] = value;
                        }
                    }
                }
            }
        }

        public string GetValue(string section, string key)
        {
            if (sections.ContainsKey(section) && sections[section].ContainsKey(key))
            {
                return sections[section][key];
            }

            return null;
        }

        public void SetValue(string section, string key, string value)
        {
            if (!sections.ContainsKey(section))
            {
                sections[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            sections[section][key] = value;
            Save();
        }

        private void Save()
        {
            List<string> lines = new List<string>();

            foreach (var section in sections)
            {
                lines.Add($"[{section.Key}]");

                foreach (var keyValuePair in section.Value)
                {
                    lines.Add($"{keyValuePair.Key}={keyValuePair.Value}");
                }

                lines.Add("");
            }

            File.WriteAllLines(filePath, lines);
        }
    }
}
