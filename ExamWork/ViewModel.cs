using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace ExamWork
{
    class ViewModel : INotifyPropertyChanged
    {

        public ViewModel()
        {
            Mutex mutex = null;
            if (!Mutex.TryOpenExisting("ExamWork", out mutex))
            {

                mutex = new Mutex(true, "ExamWork");
            }
            else { MessageBox.Show("Only one copy can be opened."); App.Current.Shutdown(); }

            selectForbiddenWordsCommand = new DelegateCommand(SelectFileWithForbiddenWords);
            selectResultFolderCommand = new DelegateCommand(SelectSaveFolderSource);
            addNewForbiddenWordCommand = new DelegateCommand(AddNewForbiddenWord, () => !String.IsNullOrWhiteSpace(NewForbiddenWord));
            removeForbiddenWordsCommand = new DelegateCommand(RemoveForbiddenWord, () => SelectedFolbiddenWord != null);
            selectSourceFolderCommand = new DelegateCommand(SelectFolderSource);

            startCommand = new DelegateCommand(Start, () => !String.IsNullOrWhiteSpace(ResultFolderPath) && !String.IsNullOrWhiteSpace(SourceFolderPath) && (IsStoped || IsAnalysingFiles != true && IsSearchingFiles != true && IsCopyingFiles != true && IsReplacingFiles != true)); ;
            pauseCommand = new DelegateCommand(Pause, () => (!IsStoped && !IsPaused) && (IsAnalysingFiles == true || IsSearchingFiles == true || IsCopyingFiles == true || IsReplacingFiles == true));
            resumeCommand = new DelegateCommand(Resume, () => IsPaused && !IsStoped);
            stopCommand = new DelegateCommand(Stop, () => !IsStoped && !IsPaused && (IsAnalysingFiles == true || IsSearchingFiles == true || IsCopyingFiles == true || IsReplacingFiles == true));

            PropertyChanged += (sender, args) =>
            {


                if (args.PropertyName == nameof(NewForbiddenWord))
                {
                    addNewForbiddenWordCommand.RaiseCanExecuteChanged();
                }
                else if (args.PropertyName == nameof(SelectedFolbiddenWord))
                {
                    removeForbiddenWordsCommand.RaiseCanExecuteChanged();
                }
                else if (args.PropertyName == nameof(ResultFolderPath))
                {
                    startCommand.RaiseCanExecuteChanged();
                }
                else if (args.PropertyName == nameof(SourceFolderPath))
                {
                    startCommand.RaiseCanExecuteChanged();
                }
                else if (args.PropertyName == nameof(ForbiddenWords))
                {
                    startCommand.RaiseCanExecuteChanged();
                }

                else if (args.PropertyName == nameof(IsStoped))
                {
                    startCommand.RaiseCanExecuteChanged();
                    resumeCommand.RaiseCanExecuteChanged();
                    pauseCommand.RaiseCanExecuteChanged();
                    stopCommand.RaiseCanExecuteChanged();
                }
                else if (args.PropertyName == nameof(IsPaused))
                {
                    resumeCommand.RaiseCanExecuteChanged();
                    pauseCommand.RaiseCanExecuteChanged();
                    stopCommand.RaiseCanExecuteChanged();
                }
                else if (args.PropertyName == nameof(IsAnalysingFiles))
                {
                    startCommand.RaiseCanExecuteChanged();
                    pauseCommand.RaiseCanExecuteChanged();
                    stopCommand.RaiseCanExecuteChanged();
                }
                else if (args.PropertyName == nameof(IsSearchingFiles))
                {
                    startCommand.RaiseCanExecuteChanged();
                    pauseCommand.RaiseCanExecuteChanged();
                    stopCommand.RaiseCanExecuteChanged();
                }
                else if (args.PropertyName == nameof(IsCopyingFiles))
                {
                    startCommand.RaiseCanExecuteChanged();
                    pauseCommand.RaiseCanExecuteChanged();
                    stopCommand.RaiseCanExecuteChanged();
                }
                else if (args.PropertyName == nameof(IsReplacingFiles))
                {
                    startCommand.RaiseCanExecuteChanged();
                    pauseCommand.RaiseCanExecuteChanged();
                    stopCommand.RaiseCanExecuteChanged();
                }

            };
            atf = new AnalysisTextFiles();
            mres = new ManualResetEventSlim(true);
            sourceToken = new CancellationTokenSource();
            token = sourceToken.Token;
            IsSearchingFiles = null;
            IsReplacingFiles = null;
            IsAnalysingFiles = null;
            IsCopyingFiles = null;
            IsStoped = false;
            IsPaused = false;
        }
        AnalysisTextFiles atf;
        AnalysisTextFiles Atf { get { return atf; } set { if (value != atf) { atf = value; OnPropertyChanged(); } } }
        #region Property

        private string resultFolderPath;
        private string sourceFolderPath;


        private string forbiddenWordsPath;
        private string selectedFolbiddenWord;
        private string newForbiddenWord;


        private ManualResetEventSlim mres;
        private CancellationTokenSource sourceToken;
        private CancellationToken token;


        private int countReplacedWords;

        public int CountReplacedWords { get { return countReplacedWords; } set { if (value != countReplacedWords) { countReplacedWords = value; OnPropertyChanged(); } } }



        private float analysisProgress;
        public float AnalysisProgress { get { return analysisProgress; } set { if (value != analysisProgress) { analysisProgress = value; OnPropertyChanged(); } } }

        private float copyingProgress;
        public float CopyingProgress { get { return copyingProgress; } set { if (value != copyingProgress) { copyingProgress = value; OnPropertyChanged(); } } }

        private float replacementProgress;
        public float ReplacementProgress { get { return replacementProgress; } set { if (value != replacementProgress) { replacementProgress = value; OnPropertyChanged(); } } }

        private bool? isReplacingFiles;

        public bool? IsReplacingFiles { get { return isReplacingFiles; } set { if (value != isReplacingFiles) { isReplacingFiles = value; OnPropertyChanged(); } } }

        private bool? isCopyingFiles;

        public bool? IsCopyingFiles { get { return isCopyingFiles; } set { if (value != isCopyingFiles) { isCopyingFiles = value; OnPropertyChanged(); } } }

        private bool? isSearchingFiles;

        public bool? IsSearchingFiles { get { return isSearchingFiles; } set { if (value != isSearchingFiles) { isSearchingFiles = value; OnPropertyChanged(); } } }

        private bool? isAnalysingFiles;

        public bool IsStoped { get { return isStoped; } set { if (value != isStoped) { isStoped = value; OnPropertyChanged(); } } }

        private bool isStoped;

        public bool IsPaused { get { return isPaused; } set { if (value != isPaused) { isPaused = value; OnPropertyChanged(); } } }

        private bool isPaused;
        public bool? IsAnalysingFiles { get { return isAnalysingFiles; } set { if (value != isAnalysingFiles) { isAnalysingFiles = value; OnPropertyChanged(); } } }


        public string SelectedFolbiddenWord { get { return selectedFolbiddenWord; } set { if (value != selectedFolbiddenWord) { selectedFolbiddenWord = value; OnPropertyChanged(); } } }
        public string NewForbiddenWord { get { return newForbiddenWord; } set { if (value != newForbiddenWord) { newForbiddenWord = value; OnPropertyChanged(); } } }


        public string ResultFolderPath { get { return resultFolderPath; } set { if (value != resultFolderPath) { resultFolderPath = value; OnPropertyChanged(); } } }
        public string ForbiddenWordsPath { get { return forbiddenWordsPath; } set { if (value != forbiddenWordsPath) { forbiddenWordsPath = value; OnPropertyChanged(); } } }
        public string SourceFolderPath { get { return sourceFolderPath; } set { if (value != sourceFolderPath) { sourceFolderPath = value; OnPropertyChanged(); } } }


        #endregion
        #region Collections
        private readonly ICollection<string> forbiddenWords = new ObservableCollection<string>();

        private readonly ICollection<FileAnalysisInfo> allTXTFiles = new ObservableCollection<FileAnalysisInfo>();

        public IEnumerable<string> ForbiddenWords => forbiddenWords;
        public IEnumerable<FileAnalysisInfo> AllTXTFiles => allTXTFiles;

        #endregion
        #region Command
        private Command selectForbiddenWordsCommand;
        private Command addNewForbiddenWordCommand;
        private Command removeForbiddenWordsCommand;


        private Command selectResultFolderCommand;
        private Command selectSourceFolderCommand;


        private Command startCommand;
        private Command pauseCommand;
        private Command stopCommand;
        private Command resumeCommand;



        public ICommand SelectForbiddenWordsCommand => selectForbiddenWordsCommand;
        public ICommand AddNewForbiddenWordCommand => addNewForbiddenWordCommand;
        public ICommand RemoveForbiddenWordsCommand => removeForbiddenWordsCommand;


        public ICommand SelectResultFolderCommand => selectResultFolderCommand;
        public ICommand SelectSourceFolderCommand => selectSourceFolderCommand;


        public ICommand StartCommand => startCommand;
        public ICommand PauseCommand => pauseCommand;
        public ICommand StopCommand => stopCommand;
        public ICommand ResumeCommand => resumeCommand;


        #endregion



        public void SelectSaveFolderSource()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                ResultFolderPath = fbd.SelectedPath;

            }
        }
        public void SelectFolderSource()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                SourceFolderPath = fbd.SelectedPath;

            }
        }
        private void AddNewForbiddenWord()
        {
            if (!forbiddenWords.Contains(newForbiddenWord) && newForbiddenWord != String.Empty)
                forbiddenWords.Add(newForbiddenWord);
            NewForbiddenWord = "";
            SetAnalysisWords();
        }
        private void RemoveForbiddenWord()
        {
            if (selectedFolbiddenWord != null && forbiddenWords.Contains(selectedFolbiddenWord))
                forbiddenWords.Remove(selectedFolbiddenWord);
            SetAnalysisWords();
        }
        private void SelectFileWithForbiddenWords()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text|*.txt|All|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ForbiddenWordsPath = ofd.FileName;
                AddForbiddenWordsFromFile();
            }
            SetAnalysisWords();
        }
        public void AddForbiddenWordsFromFile()
        {
            using (StreamReader sr = new StreamReader(File.OpenRead(forbiddenWordsPath)))
            {
                while (!sr.EndOfStream)
                {
                    string lines = sr.ReadLine();
                    foreach (var item in lines.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!forbiddenWords.Contains(item))
                            forbiddenWords.Add(item);
                    }
                }

            }
            SetAnalysisWords();
        }
        public void SetAnalysisWords()
        {
            atf.WordsCount.Clear();
            foreach (var item in forbiddenWords)
            {
                atf.WordsCount.Add(item, 0);
            }
        }

        public void TraverseTree(string root)
        {

            Stack<string> dirs = new Stack<string>(20);

            if (!Directory.Exists(root))
            {
                return;
            }
            dirs.Push(root);

            while (dirs.Count > 0)
            {
                mres.Wait();
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();


                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }

                catch { continue; }



                string[] files = null;
                try
                {
                    files = Directory.GetFiles(currentDir, "*.txt");
                }
                catch { continue; }

                foreach (string file in files)
                {
                    if (token.IsCancellationRequested)
                        token.ThrowIfCancellationRequested();
                    try
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {

                            allTXTFiles.Add(new FileAnalysisInfo() { PathToFile = file, Size = new FileInfo(file).Length });

                        }));
                    }
                    catch { continue; }
                }


                foreach (string str in subDirs)
                    dirs.Push(str);
            }
            mres.Set();
            Thread.Sleep(2000);


        }



        public void Pause()
        {

            IsPaused = true;
            mres.Reset();
        }
        public void Resume()
        {

            IsPaused = false;
            mres.Set();

        }
        public void Stop()
        {
            IsStoped = true;

            sourceToken.Cancel();
        }

        public void Start()
        {
            try
            {
                sourceToken = new CancellationTokenSource();
                token = sourceToken.Token;
                Atf = new AnalysisTextFiles();
                SetAnalysisWords();

                IsReplacingFiles = null;
                IsAnalysingFiles = null;
                IsCopyingFiles = null;
                IsStoped = false;
                IsPaused = false;
                allTXTFiles.Clear();

                IsSearchingFiles = true;
                AnalysisProgress = 0;
                CopyingProgress = 0;
                ReplacementProgress = 0;
                float percent = 0;
                Task task1 = Task.Factory.StartNew(() => TraverseTree(SourceFolderPath), token)
                                          .ContinueWith(result =>
                                          {
                                              System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                                              {
                                                  isSearchingFiles = false;
                                                  IsAnalysingFiles = true;
                                                  percent = 100f / allTXTFiles.Count;
                                              }));
                                          }, token);
                Task task2 = task1.ContinueWith(result => Parallel.ForEach(allTXTFiles, (item) =>
                {
                    atf.Analysis(item, mres, token);

                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (AnalysisProgress + percent < 100)
                            AnalysisProgress += percent;
                        else AnalysisProgress = 100;
                    }));

                }), token).ContinueWith(result =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        AnalysisProgress = 100;
                        isAnalysingFiles = false;
                        IsCopyingFiles = true;
                        percent = 100f / allTXTFiles.Where(a => a.CountWords > 0).Count() / 2f;
                    }));
                }, token);


                Task task3 = task2.ContinueWith(result => Parallel.ForEach(allTXTFiles, (item) =>
                {
                    CopyFile(item, "(Original)");

                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        CountReplacedWords += item.CountWords;
                        if (CopyingProgress + percent < 50)
                            CopyingProgress += percent;
                        else CopyingProgress = 50;
                    }));

                }), token);

                Task task4 = task3.ContinueWith(result => Parallel.ForEach(allTXTFiles, (item) =>
                {

                    CopyFile(item, "(Replace)");
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (CopyingProgress + percent < 100)
                            CopyingProgress += percent;
                        else CopyingProgress = 100;
                    }));


                }), token)
                                  .ContinueWith(result =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        CopyingProgress = 100;
                        percent = 100f / allTXTFiles.Where(a => a.CountWords > 0).Count();
                        isCopyingFiles = false;
                        IsReplacingFiles = true;
                    }));
                }, token);

                Task task5 = task4.ContinueWith(result => Parallel.ForEach(allTXTFiles, (item) =>
                {
                    atf.Replace(item, mres);
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (ReplacementProgress + percent < 100)
                            ReplacementProgress += percent;
                        else ReplacementProgress = 100;
                    }));


                }), token)
                                  .ContinueWith(result =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        ReplacementProgress = 100;
                        IsReplacingFiles = false;
                        IsStoped = true;
                    }));
                }, token);

                Task task6 = task5.ContinueWith(result => Parallel.Invoke(WriteOrder), token);



            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }





        }
        public void WriteOrder()
        {
            mres.Wait();
            using (StreamWriter sw = new StreamWriter(File.Create(Path.Combine(resultFolderPath, "Report.txt"))))
            {
                string line = "Total files found: " + allTXTFiles.Count() + "\n";

                foreach (var item in allTXTFiles)
                {
                   

                    mres.Wait();
                    if (item.CountWords > 0)
                    {
                        lock (this)
                        {
                            line += new string('-', 100);
                            line += "\n";
                            line += String.Format("File path: {0}\nCopy path: {1}\nSize: {2}\nCount words: {3}\n", item.PathToFile, item.PathToCopyFile, item.Size, item.CountWords);
                            line += new string('-', 100);
                        }
                    }
                }
                var result = atf.TakeTop10();
                line += new string('-', 100);
                line += "\nTop forbidden words:\n";
                foreach (var item in result)
                {
                    line += item.Key + " - " + item.Value + "\n";
                }

                sw.Write(line);
                sw.Close();
                mres.Set();
            }
        }
        public void CopyFile(FileAnalysisInfo file, string prefix = null)
        {
            mres.Wait();
            if (allTXTFiles.Where(a => a.PathToFile == file.PathToFile && a.CountWords > 0).FirstOrDefault() != null)
            {
                

                int index = 0;
                string tmp;
                lock (this)
                {
                    do
                    {
                        mres.Wait();
                        if (token.IsCancellationRequested)
                            token.ThrowIfCancellationRequested();
                        tmp = prefix == null ? Path.Combine(resultFolderPath, Path.GetFileNameWithoutExtension(file.PathToFile) + (index == 0 ? null : $"({index})") + Path.GetExtension(file.PathToFile)) : Path.Combine(resultFolderPath, $"{prefix}" + Path.GetFileNameWithoutExtension(file.PathToFile) + (index == 0 ? null : $"({index})") + Path.GetExtension(file.PathToFile));
                        index++;
                    } while (File.Exists(tmp));

                    file.PathToCopyFile = tmp;
                    File.Copy(file.PathToFile, tmp, false);
                }
            }

            mres.Set();
            Thread.Sleep(2000);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
