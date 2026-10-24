using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordFrequency
{
    class Program
    {
        static List<string> filePaths = new List<string>();
        static List<List<string>> words = new List<List<string>>();

        static List<string> stopWords = new List<string>();
        static List<string> globalVector = new List<string>();
        static List<List<int>> fileVectors = new List<List<int>>();

        private static char[] wordArray;
        private static int endIndex;
        private static int suffixStartIndex;
        private static int totalFiles;
        private static int totalDifferentWords;

        static int Main()
        {
            string filesFolderPath = FindFilesRootFolder();
            string stopWordsFilePath = FindStopWordsFile();

            if(stopWordsFilePath == "null") return 0;

            LoadStopWords(stopWordsFilePath);

            ExtractWords(filesFolderPath);

            RemoveNonLetters();

            MakeLowerCase();

            RemoveStopWords();

            InfinitiveForm();

            RemoveStopWords();

            BuildGlobalVector();

            BuildFileVectors();

            for (int i = 0; i < filePaths.Count; i++)
            {
                Console.WriteLine("File: " + filePaths[i]);
                Console.Write("Vector: { ");

                for (int j = 0; j < fileVectors[i].Count; j += 2)
                {
                    Console.Write(globalVector[fileVectors[i][j]] + " " + fileVectors[i][j+1]);
                    if (j < fileVectors[i].Count - 2)
                    {
                        Console.Write(", ");
                    }
                }

                Console.WriteLine(" }");
                Console.WriteLine();
            }

            Console.WriteLine("Total files: " + totalFiles);
            Console.WriteLine("Total different words: " + totalDifferentWords);

            return 0;
        }

        static string FindFilesRootFolder()
        {
            string currentFolder = AppContext.BaseDirectory;

            while (currentFolder != null)
            {
                string filesRootFolder = Path.Combine(currentFolder, "files");

                if (Directory.Exists(filesRootFolder))
                {
                    Console.WriteLine(filesRootFolder + " - exists!");
                    return filesRootFolder;
                }

                DirectoryInfo rootFolder = Directory.GetParent(currentFolder);
                if (rootFolder == null)
                {
                    break;
                }

                currentFolder = rootFolder.FullName;
                Console.WriteLine(filesRootFolder + " - doesnt exist");
            }

            return "null";
        }

        static string FindStopWordsFile()
        {
            string currentFolder = AppContext.BaseDirectory;

            while (currentFolder != null)
            {
                string stopWordsPath = Path.Combine(currentFolder, "stopwords.txt");

                if (File.Exists(stopWordsPath))
                {
                    Console.WriteLine(stopWordsPath + " - exists!");
                    return stopWordsPath;
                }

                DirectoryInfo rootFolder = Directory.GetParent(currentFolder);
                if (rootFolder == null)
                {
                    break;
                }

                currentFolder = rootFolder.FullName;
            }
            Console.WriteLine("stopwords.txt file not found.");

            return "null";
        }

        static void LoadStopWords(string path)
        {
            string text = File.ReadAllText(path);
            char[] separators = new char[] { ' ', '\n', '\r', '\t', ',' };
            string[] array = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in array)
            {
                string cleanStopWord = "";
                foreach (char c in word)
                {
                    if (char.IsLetter(c))
                    {
                        cleanStopWord = cleanStopWord + c;
                    }
                }

                cleanStopWord = cleanStopWord.ToLower();

                if (cleanStopWord != "")
                {
                    stopWords.Add(cleanStopWord);
                }
            }            
        }

        static void ExtractWords(string folderPath)
        {
            totalFiles = 0;
            if (folderPath == "null") return;

            string[] paths = Directory.GetFiles(folderPath, "*.xml", SearchOption.AllDirectories);

            foreach (string path in paths)
            {
                string text = File.ReadAllText(path);
                string textWithoutTags = RemoveXmlTags(text);

                textWithoutTags = textWithoutTags.Replace("&quot;", " ");
                textWithoutTags = textWithoutTags.Replace("&amp;", " ");
                textWithoutTags = textWithoutTags.Replace("&apos;", " ");
                textWithoutTags = textWithoutTags.Replace("&lt;", " ");
                textWithoutTags = textWithoutTags.Replace("&gt;", " ");

                char[] separators = new char[] { ' ', '\n', '\r', '\t', '-', '/', '.', ',', '\''  };
                string[] wordArray = textWithoutTags.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                
                List<string> wordList = new List<string>();
                foreach (string word in wordArray)
                {
                    wordList.Add(word);
                }

                filePaths.Add(path);
                totalFiles++;
                words.Add(wordList);
            }
        }

        static string RemoveXmlTags(string text)
        {
            StringBuilder result = new StringBuilder();
            bool insideTag = false;

            foreach (char c in text)
            {
                if (c == '<')
                {
                    insideTag = true;
                    result.Append(" ");
                }
                else if (c == '>')
                {
                    insideTag = false;
                }
                else if (insideTag == false)
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        static void RemoveNonLetters()
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

                    if (cleanWord != "" && wordLengh > 2)
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

        static void RemoveStopWords()
        {
            for (int i = 0; i < words.Count; i++)
            {
                List<string> filteredWords = new List<string>();

                for (int j = 0; j < words[i].Count; j++)
                {
                    string currentWord = words[i][j];
                    bool isStopWord = false;

                    for (int k = 0; k < stopWords.Count; k++)
                    {
                        if (currentWord == stopWords[k])
                        {
                            isStopWord = true;
                            break;
                        }
                    }

                    if (isStopWord == false && currentWord.Length > 2)
                    {
                        filteredWords.Add(currentWord);
                    }
                }

                words[i] = filteredWords;
            }
        }

        static void InfinitiveForm()
        {
            for (int i = 0; i < words.Count; i++)
            {
                for (int j = 0; j < words[i].Count; j++)
                {
                    string newWord = words[i][j];

                    if (string.IsNullOrWhiteSpace(newWord) || newWord.Length <= 2) 
                    {
                        words[i][j] = newWord;
                    }
                    else
                    {
                        wordArray = newWord.ToCharArray();

                        suffixStartIndex = 0;
                        endIndex = newWord.Length - 1;
                        Step1();
                        Step2();
                        Step3();
                        Step4();
                        Step5();
                        Step6();

                        var length = endIndex + 1;
                        words[i][j] = new String(wordArray, 0, length);
                    }
                }
            }
        }

        static void BuildGlobalVector()
        {
            totalDifferentWords = 0;
            for (int i = 0; i < words.Count; i++)
            {
                for (int j = 0; j < words[i].Count; j++)
                {
                    string w = words[i][j];

                    if (globalVector.Contains(w) == false)
                    {
                        globalVector.Add(w);
                        totalDifferentWords++;
                    }
                }
            }
        }

        static void BuildFileVectors()
        {
            for (int i = 0; i < words.Count; i++)
            {
                List<int> singleFileVector = new List<int>();
                List<string> alreadyCounted = new List<string>();

                for (int j = 0; j < words[i].Count; j++)
                {
                    string currentWord = words[i][j];

                    if (alreadyCounted.Contains(currentWord) == false)
                    {
                        alreadyCounted.Add(currentWord);

                        int globalPosition = globalVector.IndexOf(currentWord);

                        int count = 0;
                        for (int k = 0; k < words[i].Count; k++)
                        {
                            if (words[i][k] == currentWord)
                            {
                                count++;
                            }
                        }

                        singleFileVector.Add(globalPosition);
                        singleFileVector.Add(count);
                    }
                }

                fileVectors.Add(singleFileVector);
            }
        }

        private static void Step1()
        {
            if (wordArray[endIndex] == 's')
            {
                if (EndsWith("sses"))
                {
                    endIndex -= 2;
                }
                else if (EndsWith("ies"))
                {
                    SetEnd("i");
                }
                else if (wordArray[endIndex - 1] != 's')
                {
                    endIndex--;
                }
            }
            if (EndsWith("eed"))
            {
                if (MeasureConsontantSequence() > 0)
                    endIndex--;
            }
            else if ((EndsWith("ed") || EndsWith("ing")) && VowelInStem())
            {
                endIndex = suffixStartIndex;
                if (EndsWith("at"))
                    SetEnd("ate");
                else if (EndsWith("bl"))
                    SetEnd("ble");
                else if (EndsWith("iz"))
                    SetEnd("ize");
                else if (IsDoubleConsontant(endIndex))
                {
                    endIndex--;
                    int ch = wordArray[endIndex];
                    if (ch == 'l' || ch == 's' || ch == 'z')
                        endIndex++;
                }
                else if (MeasureConsontantSequence() == 1 && IsCVC(endIndex)) SetEnd("e");
            }
        }

        private static void Step2()
        {
            if (EndsWith("y") && VowelInStem())
                wordArray[endIndex] = 'i';
        }

        private static void Step3()
        {
            if (endIndex == 0) return;

            switch (wordArray[endIndex - 1])
            {
                case 'a':
                    if (EndsWith("ational")) { ReplaceEnd("ate"); break; }
                    if (EndsWith("tional")) { ReplaceEnd("tion"); }
                    break;
                case 'c':
                    if (EndsWith("enci")) { ReplaceEnd("ence"); break; }
                    if (EndsWith("anci")) { ReplaceEnd("ance"); }
                    break;
                case 'e':
                    if (EndsWith("izer")) { ReplaceEnd("ize"); }
                    break;
                case 'l':
                    if (EndsWith("bli")) { ReplaceEnd("ble"); break; }
                    if (EndsWith("alli")) { ReplaceEnd("al"); break; }
                    if (EndsWith("entli")) { ReplaceEnd("ent"); break; }
                    if (EndsWith("eli")) { ReplaceEnd("e"); break; }
                    if (EndsWith("ousli")) { ReplaceEnd("ous"); }
                    break;
                case 'o':
                    if (EndsWith("ization")) { ReplaceEnd("ize"); break; }
                    if (EndsWith("ation")) { ReplaceEnd("ate"); break; }
                    if (EndsWith("ator")) { ReplaceEnd("ate"); }
                    break;
                case 's':
                    if (EndsWith("alism")) { ReplaceEnd("al"); break; }
                    if (EndsWith("iveness")) { ReplaceEnd("ive"); break; }
                    if (EndsWith("fulness")) { ReplaceEnd("ful"); break; }
                    if (EndsWith("ousness")) { ReplaceEnd("ous"); }
                    break;
                case 't':
                    if (EndsWith("aliti")) { ReplaceEnd("al"); break; }
                    if (EndsWith("iviti")) { ReplaceEnd("ive"); break; }
                    if (EndsWith("biliti")) { ReplaceEnd("ble"); }
                    break;
                case 'g':
                    if (EndsWith("logi"))
                    {
                        ReplaceEnd("log");
                    }
                    break;
            }
        }

        private static void Step4()
        {
            switch (wordArray[endIndex])
            {
                case 'e':
                    if (EndsWith("icate")) { ReplaceEnd("ic"); break; }
                    if (EndsWith("ative")) { ReplaceEnd(""); break; }
                    if (EndsWith("alize")) { ReplaceEnd("al"); }
                    break;
                case 'i':
                    if (EndsWith("iciti")) { ReplaceEnd("ic"); }
                    break;
                case 'l':
                    if (EndsWith("ical")) { ReplaceEnd("ic"); break; }
                    if (EndsWith("ful")) { ReplaceEnd(""); }
                    break;
                case 's':
                    if (EndsWith("ness")) { ReplaceEnd(""); }
                    break;
            }
        }

        private static void Step5()
        {
            if (endIndex == 0) return;

            switch (wordArray[endIndex - 1])
            {
                case 'a':
                    if (EndsWith("al")) break; return;
                case 'c':
                    if (EndsWith("ance")) break;
                    if (EndsWith("ence")) break; return;
                case 'e':
                    if (EndsWith("er")) break; return;
                case 'i':
                    if (EndsWith("ic")) break; return;
                case 'l':
                    if (EndsWith("able")) break;
                    if (EndsWith("ible")) break; return;
                case 'n':
                    if (EndsWith("ant")) break;
                    if (EndsWith("ement")) break;
                    if (EndsWith("ment")) break;
                    if (EndsWith("ent")) break; return;
                case 'o':
                    if (EndsWith("ion") && suffixStartIndex >= 0 && (wordArray[suffixStartIndex] == 's' || wordArray[suffixStartIndex] == 't')) break;
                    if (EndsWith("ou")) break; return;
                case 's':
                    if (EndsWith("ism")) break; return;
                case 't':
                    if (EndsWith("ate")) break;
                    if (EndsWith("iti")) break; return;
                case 'u':
                    if (EndsWith("ous")) break; return;
                case 'v':
                    if (EndsWith("ive")) break; return;
                case 'z':
                    if (EndsWith("ize")) break; return;
                default:
                    return;
            }
            if (MeasureConsontantSequence() > 1)
                endIndex = suffixStartIndex;
        }

        private static void Step6()
        {
            suffixStartIndex = endIndex;

            if (wordArray[endIndex] == 'e')
            {
                var a = MeasureConsontantSequence();
                if (a > 1 || a == 1 && !IsCVC(endIndex - 1))
                    endIndex--;
            }
            if (wordArray[endIndex] == 'l' && IsDoubleConsontant(endIndex) && MeasureConsontantSequence() > 1)
                endIndex--;
        }

        private static bool IsConsonant(int index)
        {
            var c = wordArray[index];
            if (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u') return false;
            return c != 'y' || (index == 0 || !IsConsonant(index - 1));
        }

        private static int MeasureConsontantSequence()
        {
            var n = 0;
            var index = 0;
            while (true)
            {
                if (index > suffixStartIndex) return n;
                if (!IsConsonant(index)) break; index++;
            }
            index++;
            while (true)
            {
                while (true)
                {
                    if (index > suffixStartIndex) return n;
                    if (IsConsonant(index)) break;
                    index++;
                }
                index++;
                n++;
                while (true)
                {
                    if (index > suffixStartIndex) return n;
                    if (!IsConsonant(index)) break;
                    index++;
                }
                index++;
            }
        }

        private static bool VowelInStem()
        {
            int i;
            for (i = 0; i <= suffixStartIndex; i++)
            {
                if (!IsConsonant(i)) return true;
            }
            return false;
        }

        private static bool IsDoubleConsontant(int index)
        {
            if (index < 1) return false;
            return wordArray[index] == wordArray[index - 1] && IsConsonant(index);
        }

        private static bool IsCVC(int index)
        {
            if (index < 2 || !IsConsonant(index) || IsConsonant(index - 1) || !IsConsonant(index - 2)) return false;
            var c = wordArray[index];
            return c != 'w' && c != 'x' && c != 'y';
        }

        private static bool EndsWith(string s)
        {
            var length = s.Length;
            var index = endIndex - length + 1;
            if (index < 0) return false;

            for (var i = 0; i < length; i++)
            {
                if (wordArray[index + i] != s[i]) return false;
            }
            suffixStartIndex = endIndex - length;
            return true;
        }

        private static void SetEnd(string s)
        {
            var length = s.Length;
            var index = suffixStartIndex + 1;
            for (var i = 0; i < length; i++)
            {
                wordArray[index + i] = s[i];
            }
            endIndex = suffixStartIndex + length;
        }

        private static void ReplaceEnd(string s)
        {
            if (MeasureConsontantSequence() > 0) SetEnd(s);
        }
    }
}