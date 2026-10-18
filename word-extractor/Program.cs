class Program
{
    static List<string> filePaths = new List<string>();
    static List<List<string>> words = new List<List<string>>();

    static void Main()
    {
        string filesFolderPath = FindFilesRootFolder();

        ExtractWords(filesFolderPath);

        RemoveNonLetters();

        MakeLowerCase();

        for (int i = 0; i < words.Count; i++)
        {
            Console.WriteLine();
            Console.WriteLine("File: " + filePaths[i]);
            Console.WriteLine("Number of words: " + words[i].Count);
            Console.WriteLine(" ");

            for (int j = 0; j < words[i].Count; j++)
            {
                Console.Write("  " + words[i][j]);
            }

            Console.WriteLine(" ");
        }
    }

    static string FindFilesRootFolder()
    {
        string currentFolder = AppContext.BaseDirectory;

        while(currentFolder != null)
        {
            string filesRootFolder = Path.Combine(currentFolder, "files");

            if(Directory.Exists(filesRootFolder))
            {
                Console.WriteLine(filesRootFolder + " - exists!");
                return filesRootFolder;
            }

            DirectoryInfo rootFolder = Directory.GetParent(currentFolder);

            currentFolder = rootFolder.FullName;

            Console.WriteLine(filesRootFolder + " - doesnt exist");
        }

        return "null";        
    }

    static void ExtractWords(string folderPath)
    {
        string[] paths = Directory.GetFiles(folderPath, "*.xml", SearchOption.AllDirectories);

        foreach (string path in paths)
        {
            string text = File.ReadAllText(path);
            string textWithoutTags = RemoveXmlTags(text);

            string[] wordArray = textWithoutTags.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            List<string> wordList = new List<string>();
            foreach (string word in wordArray)
            {
                wordList.Add(word);
            }

            filePaths.Add(path);
            words.Add(wordList);
        }
    }

    static string RemoveXmlTags(string text) //removing xml <...> tags
    {
        string result = "";
        bool insideTag = false;

        foreach (char c in text)
        {
            if (c == '<')
            {
                insideTag = true;
                result += " ";
            }
            else if (c == '>')
            {
                insideTag = false;
            }
            else if (insideTag == false)
            {
                result += c;
            }
        }

        return result;
    }

    static void RemoveNonLetters() //removing special characters and numbers from words like AT&amp;T
    {
        for (int i = 0; i < words.Count; i++)
        {
            List<string> newWords = new List<string>();

            foreach (string word in words[i])
            {
                int wordLengh = 0;
                string cleanWord = "";

                foreach (char c in word)
                {
                    if (char.IsLetter(c))
                    {
                        cleanWord = cleanWord + c;
                        wordLengh++;
                    }
                }

                if (cleanWord != "" && wordLengh > 2) //remove words smaller than 2 characters
                {
                    newWords.Add(cleanWord);
                }
            }

            words[i] = newWords;
        }
    }

    static void MakeLowerCase()
    {
        for (int i = 0; i < words.Count; i++)
        {
            for (int j = 0; j < words[i].Count; j++)
            {
                words[i][j] = words[i][j].ToLower();
            }
        }
    }
}