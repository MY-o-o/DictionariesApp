using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace DictionariesApp
{
    public class DictionaryManager
    {
        private string DictionariesFolder = "Dictionaries";

        public DictionaryManager() { }
        public DictionaryManager(string dictFolder) { DictionariesFolder = dictFolder; }

        public void Run()
        {
            if (!Directory.Exists(DictionariesFolder))
                Directory.CreateDirectory(DictionariesFolder);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Dictionaries Application Menu ===");
                Console.WriteLine("1. Create a dictionary");
                Console.WriteLine("2. Choose the dictionary");
                Console.WriteLine("3. Exit");
                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine()?.Trim();

                Console.Clear();
                switch (choice)
                {
                    case "1":
                        CreateDictionary();
                        break;
                    case "2":
                        RunDictionary();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void RunDictionary()
        {
            Console.Write("Choose the type of dictionary: ");
            string type = Console.ReadLine()?.Trim();
            var dictionary = LoadDictionary(type);
            if (dictionary == null)
            {
                return;
            }
            Console.WriteLine("Success! Dictionary found.");
            Pause();

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== {type} Dictionary Menu ===");
                Console.WriteLine("1. Show the dictionary");
                Console.WriteLine("2. Add a word and translation");
                Console.WriteLine("3. Replace a word or translation");
                Console.WriteLine("4. Delete a word or translation");
                Console.WriteLine("5. Search for a translation");
                Console.WriteLine("6. Export a word and its translations");
                Console.WriteLine("7. Exit to main menu");
                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine()?.Trim();

                Console.Clear();
                switch (choice)
                {
                    case "1":
                        Console.Write("Enter a limit (0 for no limit): ");
                        int limit = int.Parse(Console.ReadLine()?.Trim());
                        if (limit <= 0) ShowDictionary(ref dictionary);
                        else ShowDictionary(ref dictionary, limit);
                        break;
                    case "2":
                        AddWord(ref dictionary, type);
                        break;
                    case "3":
                        ReplaceWordOrTranslation(ref dictionary, type);
                        break;
                    case "4":
                        DeleteWordOrTranslation(ref dictionary, type);
                        break;
                    case "5":
                        SearchTranslation(ref dictionary);
                        break;
                    case "6":
                        ExportWord(ref dictionary);
                        break;
                    case "7":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void CreateDictionary()
        {
            Console.Write("Enter the type of dictionary (e.g., English-German): ");
            string type = Console.ReadLine()?.Trim();
            if (type == string.Empty)
            {
                Console.WriteLine("Dictionary type cannot be empty!");
                Pause();
                return;
            }

            string path = GetDictionaryPath(type);

            if (File.Exists(path))
            {
                Console.WriteLine("Dictionary already exists!");
            }
            else
            {
                var xml = new XElement("Dictionary", new XAttribute("Type", type));
                xml.Save(path);
                Console.WriteLine("Dictionary created successfully!");
            }

            Pause();
        }

        private void AddWord(ref XElement dictionary, string type)
        {
            Console.Write("Enter the word: ");
            string word = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(word))
            {
                Console.WriteLine("Word cannot be empty!");
                Pause();
                return;
            }

            Console.Write("Enter the translations (comma-separated): ");
            var translations = Console.ReadLine()?.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();

            if (translations == null || translations.Count == 0)
            {
                Console.WriteLine("Translations cannot be empty!");
                Pause();
                return;
            }

            var existingWord = dictionary.Elements("Word").FirstOrDefault(w => w.Attribute("Text")?.Value == word);
            if (existingWord == null)
            {
                var wordElement = new XElement("Word", new XAttribute("Text", word));
                foreach (var translation in translations)
                {
                    wordElement.Add(new XElement("Translation", translation));
                }
                dictionary.Add(wordElement);
            }
            else
            {
                foreach (var translation in translations)
                {
                    if (!existingWord.Elements("Translation").Any(t => t.Value == translation))
                    {
                        existingWord.Add(new XElement("Translation", translation));
                    }
                }
            }

            SaveDictionary(dictionary, type);
            Console.WriteLine("Word and translations added!");
            Pause();
        }

        private void ReplaceWordOrTranslation(ref XElement dictionary, string type)
        {
            Console.Write("Enter the word: ");
            string word = Console.ReadLine()?.Trim();

            var wordElement = dictionary.Elements("Word").FirstOrDefault(w => w.Attribute("Text")?.Value == word);
            if (wordElement == null)
            {
                Console.WriteLine("Word not found.");
                Pause();
                return;
            }

            Console.WriteLine("1. Replace the word");
            Console.WriteLine("2. Replace a translation");
            Console.WriteLine("3. Exit");
            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    Console.Write("Enter the new word: ");
                    string newWord = Console.ReadLine()?.Trim();
                    if (string.IsNullOrWhiteSpace(newWord))
                    {
                        Console.WriteLine("New word cannot be empty!");
                        break;
                    }
                    wordElement.SetAttributeValue("Text", newWord);
                    break;

                case "2":
                    Console.WriteLine("Translations: " + string.Join(", ", wordElement.Elements("Translation").Select(t => t.Value)));
                    Console.Write("Enter the translation to replace: ");
                    string oldTranslation = Console.ReadLine()?.Trim();

                    var translationElement = wordElement.Elements("Translation").FirstOrDefault(t => t.Value == oldTranslation);
                    if (translationElement == null)
                    {
                        Console.WriteLine("Translation not found.");
                        break;
                    }

                    Console.Write("Enter the new translation: ");
                    string newTranslation = Console.ReadLine()?.Trim();
                    if (string.IsNullOrWhiteSpace(newTranslation))
                    {
                        Console.WriteLine("New translation cannot be empty!");
                        break;
                    }
                    translationElement.Value = newTranslation;
                    break;

                case "3":
                    return;

                default:
                    Console.WriteLine("Invalid choice.");
                    Pause();
                    return;
            }

            SaveDictionary(dictionary, type);
            Console.WriteLine("Changes saved.");
            Pause();
        }

        private void DeleteWordOrTranslation(ref XElement dictionary, string type)
        {
            Console.Write("Enter the word: ");
            string word = Console.ReadLine()?.Trim();

            var wordElement = dictionary.Elements("Word").FirstOrDefault(w => w.Attribute("Text")?.Value == word);
            if (wordElement == null)
            {
                Console.WriteLine("Word not found.");
                Pause();
                return;
            }

            Console.WriteLine("1. Delete the word");
            Console.WriteLine("2. Delete a translation");
            Console.WriteLine("3. Exit");
            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    wordElement.Remove();
                    break;

                case "2":
                    Console.WriteLine("Translations: " + string.Join(", ", wordElement.Elements("Translation").Select(t => t.Value)));
                    Console.Write("Enter the translation to delete: ");
                    string translation = Console.ReadLine()?.Trim();

                    var translationElement = wordElement.Elements("Translation").FirstOrDefault(t => t.Value == translation);
                    if (translationElement == null)
                    {
                        Console.WriteLine("Translation not found.");
                        break;
                    }

                    if (wordElement.Elements("Translation").Count() == 1)
                    {
                        Console.WriteLine("Cannot delete the last translation.");
                        break;
                    }

                    translationElement.Remove();
                    break;

                case "3":
                    return;

                default:
                    Console.WriteLine("Invalid choice.");
                    Pause();
                    return;
            }

            SaveDictionary(dictionary, type);
            Console.WriteLine("Changes saved.");
            Pause();
        }

        private void SearchTranslation(ref XElement dictionary)
        {
            Console.Write("Enter the word to search for: ");
            string word = Console.ReadLine()?.Trim();

            var wordElement = dictionary.Elements("Word").FirstOrDefault(w => w.Attribute("Text")?.Value == word);
            if (wordElement != null)
            {
                Console.WriteLine("Translations: " + string.Join(", ", wordElement.Elements("Translation").Select(t => t.Value)));
            }
            else
            {
                Console.WriteLine("Word not found.");
            }

            Pause();
        }

        private void ShowDictionary(ref XElement dictionary)
        {
            Console.WriteLine("=== Dictionary Content ===");

            var words = dictionary.Elements("Word");
            if (!words.Any())
            {
                Console.WriteLine("The dictionary is empty.");
            }
            else
            {
                foreach (var wordElement in words)
                {
                    Console.WriteLine($"{wordElement.Attribute("Text")?.Value}: {string.Join(", ", wordElement.Elements("Translation").Select(t => t.Value))}");
                }
            }

            Pause();
        }

        private void ShowDictionary(ref XElement dictionary, int limit)
        {
            if (limit <= 0)
            {
                Console.WriteLine("Invalid limit.");
                Pause();
                return;
            }

            Console.WriteLine("=== Dictionary Content ===");

            var words = dictionary.Elements("Word");
            if (!words.Any())
            {
                Console.WriteLine("The dictionary is empty.");
            }
            else
            {
                foreach (var wordElement in words)
                {
                    if (limit == 0) break;
                    Console.WriteLine($"{wordElement.Attribute("Text")?.Value}: {string.Join(", ", wordElement.Elements("Translation").Select(t => t.Value))}");
                    limit--;
                }
            }

            Pause();
        }

        private void ExportWord(ref XElement dictionary)
        {
            Console.Write("Enter the word to export: ");
            string word = Console.ReadLine()?.Trim();

            var wordElement = dictionary.Elements("Word").FirstOrDefault(w => w.Attribute("Text")?.Value == word);
            if (wordElement == null)
            {
                Console.WriteLine("Word not found.");
                Pause();
                return;
            }

            string exportPath = $"{word}_translations.xml";
            var exportXml = new XElement("Word",
                new XAttribute("Text", word),
                wordElement.Elements("Translation"));
            exportXml.Save(exportPath);
            Console.WriteLine($"Exported to file: {exportPath}");
            Pause();
        }

        private XElement LoadDictionary(string type)
        {
            string path = GetDictionaryPath(type);

            if (!File.Exists(path))
            {
                Console.WriteLine("Dictionary not found.");
                Pause();
                return null;
            }

            return XElement.Load(path);
        }

        private void SaveDictionary(XElement dictionary, string type)
        {
            string path = GetDictionaryPath(type);
            dictionary.Save(path);
        }

        private string GetDictionaryPath(string type)
        {
            return Path.Combine(DictionariesFolder, $"{type}.xml");
        }

        private void Pause()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}