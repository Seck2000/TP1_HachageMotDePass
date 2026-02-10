using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AppDictionnaire
{
    public class MainViewModel : ViewModelBase
    {
        private int _minLength = 3;
        private int _maxLength = 3;
        private bool _useLowerCase = true;
        private bool _useUpperCase;
        private bool _useNumbers;
        private bool _useSpecial;
        private string _customChars = "";
        private string _outputPath = "";
        private int _progressValue;
        private string _statusMessage = "Prêt.";
        private bool _isBusy;

        public int MinLength
        {
            get => _minLength;
            set => SetProperty(ref _minLength, value);
        }

        public int MaxLength
        {
            get => _maxLength;
            set => SetProperty(ref _maxLength, value);
        }

        public bool UseLowerCase
        {
            get => _useLowerCase;
            set => SetProperty(ref _useLowerCase, value);
        }

        public bool UseUpperCase
        {
            get => _useUpperCase;
            set => SetProperty(ref _useUpperCase, value);
        }

        public bool UseNumbers
        {
            get => _useNumbers;
            set => SetProperty(ref _useNumbers, value);
        }

        public bool UseSpecial
        {
            get => _useSpecial;
            set => SetProperty(ref _useSpecial, value);
        }

        public string CustomChars
        {
            get => _customChars;
            set => SetProperty(ref _customChars, value);
        }

        public string OutputPath
        {
            get => _outputPath;
            set => SetProperty(ref _outputPath, value);
        }

        public int ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set 
            {
                if (SetProperty(ref _isBusy, value))
                {
                    // Force re-evaluation of commands
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ICommand BrowseCommand { get; }
        public ICommand GenerateCommand { get; }

        public MainViewModel()
        {
            BrowseCommand = new RelayCommand(ExecuteBrowse, CanExecuteUI);
            GenerateCommand = new RelayCommand(ExecuteGenerate, CanExecuteGenerate);
        }

        private bool CanExecuteUI(object? parameter) => !IsBusy;

        private bool CanExecuteGenerate(object? parameter)
        {
            return !IsBusy && !string.IsNullOrWhiteSpace(OutputPath);
        }

        private void ExecuteBrowse(object? parameter)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Fichiers texte (*.txt)|*.txt",
                FileName = "dictionnaire.txt"
            };

            if (sfd.ShowDialog() == true)
            {
                OutputPath = sfd.FileName;
            }
        }

        private async void ExecuteGenerate(object? parameter)
        {
            if (MinLength > MaxLength)
            {
                MessageBox.Show("La longueur minimale ne peut pas être supérieure à la maximale.");
                return;
            }

            string charset = GetCharset();
            if (string.IsNullOrEmpty(charset))
            {
                MessageBox.Show("Veuillez sélectionner des caractères ou entrer une chaîne personnalisée.");
                return;
            }

            IsBusy = true;
            ProgressValue = 0;
            StatusMessage = "Génération en cours...";

            try
            {
                await Task.Run(() => GenerateDictionary(charset, MinLength, MaxLength, OutputPath));
                StatusMessage = "Terminé !";
                MessageBox.Show("Dictionnaire généré avec succès !");
            }
            catch (Exception ex)
            {
                StatusMessage = "Erreur !";
                MessageBox.Show($"Une erreur est survenue : {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                ProgressValue = 100;
            }
        }

        private string GetCharset()
        {
            if (!string.IsNullOrEmpty(CustomChars))
                return CustomChars;

            StringBuilder sb = new StringBuilder();
            if (UseLowerCase) sb.Append("abcdefghijklmnopqrstuvwxyz");
            if (UseUpperCase) sb.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            if (UseNumbers) sb.Append("0123456789");
            if (UseSpecial) sb.Append("!@#$%^&*()-_=+[]{}|;:,.<>?");
            return sb.ToString();
        }

        private void GenerateDictionary(string charset, int min, int max, string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                long totalCombinations = 0;
                for (int len = min; len <= max; len++)
                {
                    totalCombinations += (long)Math.Pow(charset.Length, len);
                }

                long count = 0;
                int lastPercent = 0;

                for (int len = min; len <= max; len++)
                {
                    GenerateRecursive(writer, "", len, charset, ref count, totalCombinations, ref lastPercent);
                }
            }
        }

        private void GenerateRecursive(StreamWriter writer, string current, int targetLen, string charset, ref long count, long total, ref int lastPercent)
        {
            if (current.Length == targetLen)
            {
                writer.WriteLine(current);
                count++;

                if (count % 1000 == 0)
                {
                    int percent = (int)((double)count / total * 100);
                    if (percent > lastPercent)
                    {
                        lastPercent = percent;
                        // Use Application.Current.Dispatcher to update UI thread
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ProgressValue = percent;
                            StatusMessage = $"Génération... {percent}%";
                        });
                    }
                }
                return;
            }

            foreach (char c in charset)
            {
                GenerateRecursive(writer, current + c, targetLen, charset, ref count, total, ref lastPercent);
            }
        }
    }
}
