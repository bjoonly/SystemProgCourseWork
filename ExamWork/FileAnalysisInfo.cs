using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExamWork
{
    class FileAnalysisInfo : INotifyPropertyChanged
    {
        private string pathToFile;
        private string pathToCopyFile;
        private double size;
        private int countWords;

        public string PathToFile { get { return pathToFile; } set { if (value != pathToFile) { pathToFile = value;OnPropertyChanged(); } } }
        public string PathToCopyFile { get { return pathToCopyFile; } set { if (value != pathToCopyFile) { pathToCopyFile = value; OnPropertyChanged(); } } }

        public double Size { get { return size; }set { if (value != size) { size = value;OnPropertyChanged(); } } }
        public int CountWords { get { return countWords; } set { if (value != countWords){ countWords = value;OnPropertyChanged(); } } }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
