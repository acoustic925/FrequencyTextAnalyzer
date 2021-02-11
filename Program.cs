using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace FrequencyTextAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            bool startFlag = false;
            string filePath = null;

            Console.WriteLine("End the programm before starting: exit");

            Console.WriteLine("Please write file path: ");

            while (startFlag == false)
            {
                
                filePath = $@"{Console.ReadLine()}"; // Храним в переменной filePath путь к текстовому файлу

                if(File.Exists(filePath)) // Проверка существования файла
                {
                    startFlag = true;
                }
                else if(filePath == "exit") // условие для выхода из цикла
                {
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                    break;
                }
                else // Просим повторить ввод пути для файла, в случае если файла не существует
                {                    
                    Console.WriteLine($"Can't find the path:{filePath}");                   
                    Console.WriteLine("Please write new file path: ");
                }
            }

            if (startFlag == true)
            {
                Thread thread = new Thread(() => FindTripletsInSomeTextFile(filePath)); // Создается поток thread с делегатом findTripletsInTextFile в конструкторе

                thread.Start(); // Запускается поток, в него передается переменная filePath          

                if (Console.ReadKey() != null && thread.IsAlive) // Отслеживается нажатие любой клавиши, кроме управляющих клавиш alt, ctrl, shif, комбинации alt+, shift+, ctrl+ допустимы для прерывания.
                {
                    thread.Abort(); // Прерывается поток
                    Console.ReadKey();
                }

                thread.Join();
            }

        }

        static void FindTripletsInSomeTextFile(string filePath) 
        {
            
            var timer = System.Diagnostics.Stopwatch.StartNew(); // Запускаем таймер

            Dictionary<string, int> tripletsInTextFile = new Dictionary<string, int>(); // Словарь куда будут записываться триплеты

            try // отслеживаем прерывание потока - ThreadAbortException
            {               
                List<string> linesFromFile = File.ReadAllLines(filePath).ToList<string>(); // По строчно читается весь файл

                foreach(string line in linesFromFile)// Цикл для анализа каждой строки
                {
                    if(line.Length > 0)
                    {
                        List<string> tripletsInLine =  FindTripletsInLine(line); // Ищет триплеты в строке

                        if(tripletsInLine.Count > 0)
                        {
                            FillTripletsDictionary(ref tripletsInTextFile, tripletsInLine); // Записывает триплеты в словарь
                        }
                    }
                }           
            }
            catch(ThreadAbortException)
            {
                Console.WriteLine("The thread was aborted");
            }
            finally
            {
                if (tripletsInTextFile.Count > 0)
                {
                    Dictionary<string, int> sortedTripletInTextFile = (from pair in tripletsInTextFile
                                                  orderby pair.Value descending                                                 
                                                  select pair).Take(10).ToDictionary(pair => pair.Key, pair => pair.Value); // Сортирует словарь триплетов и выбирает первые 10

                    string lineOut = WriteLineOut(sortedTripletInTextFile); // Формирует строку для вывода в консоль

                    Console.WriteLine(lineOut);
                }
                else
                {
                    Console.WriteLine($@"Couldn't find triplets in the file {Path.GetFileName(filePath)}");
                }

                timer.Stop();
                Console.WriteLine(timer.ElapsedMilliseconds); // Выводит время таймера в милисекундах
            }
        }

        static List<string> FindTripletsInLine(in string line) // Осуществляет поиск триплетов в строке
        {
            List<string> tripletsInLine = new List<string>();
            
            string[] wordsInLine = line.Split();

            foreach (string word in wordsInLine)
            {
                if (word.Length >= 3)
                {
                    List<string> tripletsInWord = FindTripletsInWord(word);

                    if (tripletsInWord.Count > 0)
                    {
                        tripletsInLine = tripletsInLine.Concat(tripletsInWord).ToList<string>();
                    }
                }
            }

            return tripletsInLine;
        }

        static List<string> FindTripletsInWord(string word) // осуществляет поиск триплетов в слове
        {
            List<string> tripletsInWord = new List<string>();

            char[] letterInWord = word.ToCharArray();

            for(int i = 0; i < letterInWord.Length - 2; i++)
            {
                if (IsEqual(letterInWord[i], letterInWord[i + 1], letterInWord[i + 2]) && IsLetter(letterInWord[i], letterInWord[i + 1], letterInWord[i + 2])) // Проверка на эквивалентность знаков и на соответствие символов категории букв
                {
                    if (i + 3 < letterInWord.Length)
                    {
                        if (!IsQuarterlet(letterInWord[i], letterInWord[i + 1], letterInWord[i + 2], letterInWord[i + 3]))
                        {
                            tripletsInWord.Add($"{letterInWord[i]}{letterInWord[i + 1]}{letterInWord[i + 2]}");
                        }
                    }
                    else
                    {
                        tripletsInWord.Add($"{letterInWord[i]}{letterInWord[i + 1]}{letterInWord[i + 2]}");
                    }
                }
            }           
            return tripletsInWord;           
        }

        static void FillTripletsDictionary(ref Dictionary<string, int> triplexInTextFile, in List<string> tripletsInLine) // Заполняет словарь триплетов
        {
            foreach(string triplet in tripletsInLine)
            {
                if(triplexInTextFile.ContainsKey(triplet))
                {
                    triplexInTextFile[triplet]++;
                }
                else
                {
                    triplexInTextFile.Add(triplet, 1);
                }
            }
        }

        static string WriteLineOut(in Dictionary<string, int> sortedTripletInTextFile) // Создает строку для вывода в консоль
        {
            string lineOut = null;

            for (int i = 0; i < sortedTripletInTextFile.Count - 1; i++)
            {
                lineOut += sortedTripletInTextFile.ElementAt(i).Key + sortedTripletInTextFile.ElementAt(i).Value.ToString() +  ", ";
            }

            lineOut += sortedTripletInTextFile.ElementAt(sortedTripletInTextFile.Count - 1).Key;

            return lineOut;
        }       

        static bool IsLetter(char letter1, char letter2, char letter3) => (letter1 == letter2 && letter1 == letter3); // Проверка на эквивалентность символов
        static bool IsEqual(char letter1, char letter2, char letter3) => (Char.IsLetter(letter1) && Char.IsLetter(letter2) && Char.IsLetter(letter3)); // Проверка на принадлежность символов к буквенному типу
        static bool IsQuarterlet(char letter1, char letter2, char letter3, char letter4) => (letter1 == letter2 && letter1 == letter3 && letter1 == letter4);
    }
}
