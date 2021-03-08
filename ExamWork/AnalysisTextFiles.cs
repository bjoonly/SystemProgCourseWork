using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;

namespace ExamWork
{
    class AnalysisTextFiles : INotifyPropertyChanged
    {


        public Dictionary<string, int> WordsCount { get; set; }
        public AnalysisTextFiles()
        {
            WordsCount = new Dictionary<string, int>();
        }

        public void Analysis(FileAnalysisInfo fai, ManualResetEventSlim mres, CancellationToken token)
        {
            string[] TempArray = new string[WordsCount.Count];
            WordsCount.Keys.CopyTo(TempArray, 0);

            using (StreamReader sr = new StreamReader(File.OpenRead(fai.PathToFile)))
            {

                mres.Wait();
                while (!sr.EndOfStream)
                {
                    if (token.IsCancellationRequested)
                        return;
                    string text = sr.ReadLine();


                    string[] words = text.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in words)
                    {

                        foreach (var item in TempArray)
                        {

                            if (token.IsCancellationRequested)
                                return;

                            if (word.Contains(item.ToString()))
                            {
                                lock (this)
                                {
                                    if (token.IsCancellationRequested)
                                        token.ThrowIfCancellationRequested();
                                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        WordsCount[item.ToString()]++;
                                        fai.CountWords++;
                                    }));
                                }

                            }
                        }
                    }
                }
                mres.Set();
                Thread.Sleep(2000);
            }

        }
        public void Replace(FileAnalysisInfo fai, ManualResetEventSlim mres)
        {

            using (StreamReader sr = new StreamReader(File.OpenRead(fai.PathToFile)))
            {

                mres.Wait();
                string result = "";
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string []words = SplitToWords(line);
            
                    for (int i = 0; i < words.Length; i++)
                    {
                        {

                            foreach (var item in WordsCount.Keys)
                            {
                                if (words[i].ToLower().Contains(item.ToLower()))
                                    words[i] = "*******";
                            }

                        }

                    }
                    foreach (var item in words)
                    {
                        result += item ;
                    }
                    result += "\n";
                }
                using (StreamWriter sw = new StreamWriter(File.OpenWrite(fai.PathToCopyFile)))
                {
                    sw.Write(result);
                    sw.Close();
                }
                mres.Set();
                sr.Close();

            }
        }
        public string[] SplitToWords(string line)
        {
            int index = 0;
            List<string> result = new List<string>();
            string word="";
            char[] sep = new char[] {',','.','?','!',' ' };
            while (index < line.Length)
            {
                if (sep.Contains(line[index]))
                {
                    result.Add(line[index].ToString());
                    index++;
                    continue;
                }
                while(!sep.Contains(line[index]))
                {
                    word += line[index];
                    index++;
                }
                result.Add(word);
                word = "";
            }


            return result.ToArray();
        }
        public IEnumerable<KeyValuePair<string, int>> TakeTop10()
        {
            List<KeyValuePair<string, int>> myList = WordsCount.ToList();


            myList.Sort(
               delegate (KeyValuePair<string, int> pair1,
               KeyValuePair<string, int> pair2)
               {
                   return pair1.Value.CompareTo(pair2.Value);
               });
            myList.Reverse();
            return myList.Take(10);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
