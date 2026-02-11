using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace AppHachage
{
    public class HachageViewModel : ViewModelBase
    {
        private readonly DispatcherTimer _elapsedTimer;
        private readonly Stopwatch _stopwatch = new();
        private CancellationTokenSource? _cancellationSource;

        private string _hashToCrack = string.Empty;
        private string _dictionaryPath = string.Empty;
        private int _dictionaryCount;
        private int _attemptCount;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private string _statusMessage = "En attente de validation.";
        private string? _foundPassword;
        private string? _foundSalt;
        private bool _isProcessing;

        public HachageViewModel()
        {
            _elapsedTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _elapsedTimer.Tick += (_, _) => ElapsedTime = _stopwatch.Elapsed;

            BrowseDictionaryCommand = new RelayCommand(BrowseDictionary, () => !IsProcessing);
            StartValidationCommand = new RelayCommand(async () => await StartValidationAsync(), CanStartValidation);
            CancelValidationCommand = new RelayCommand(CancelValidation, () => IsProcessing);
        }

        public string HashToCrack
        {
            get => _hashToCrack;
            set
            {
                if (SetProperty(ref _hashToCrack, value))
                {
                    RaiseCommandStates();
                }
            }
        }

        public string DictionaryPath
        {
            get => _dictionaryPath;
            set
            {
                if (SetProperty(ref _dictionaryPath, value))
                {
                    _ = UpdateDictionaryMetadataAsync();
                    RaiseCommandStates();
                }
            }
        }

        public int DictionaryCount
        {
            get => _dictionaryCount;
            private set => SetProperty(ref _dictionaryCount, value);
        }

        public int AttemptCount
        {
            get => _attemptCount;
            private set => SetProperty(ref _attemptCount, value);
        }

        public TimeSpan ElapsedTime
        {
            get => _elapsedTime;
            private set
            {
                if (SetProperty(ref _elapsedTime, value))
                {
                    OnPropertyChanged(nameof(ElapsedTimeDisplay));
                }
            }
        }

        public string ElapsedTimeDisplay => ElapsedTime.ToString(@"hh\:mm\:ss");

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public string? FoundPassword
        {
            get => _foundPassword;
            private set => SetProperty(ref _foundPassword, value);
        }

        public string? FoundSalt
        {
            get => _foundSalt;
            private set => SetProperty(ref _foundSalt, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            private set
            {
                if (SetProperty(ref _isProcessing, value))
                {
                    RaiseCommandStates();
                }
            }
        }

        public ICommand BrowseDictionaryCommand { get; }
        public ICommand StartValidationCommand { get; }
        public ICommand CancelValidationCommand { get; }

        private async Task StartValidationAsync()
        {
            if (IsProcessing)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(HashToCrack))
            {
                StatusMessage = "Veuillez saisir un hachage bcrypt valide.";
                return;
            }

            if (!File.Exists(DictionaryPath))
            {
                StatusMessage = "Le fichier dictionnaire est introuvable.";
                return;
            }

            var salt = TryExtractSalt(HashToCrack);
            if (salt is null)
            {
                StatusMessage = "Format de hachage non reconnu (attendu : bcrypt coût 10).";
                return;
            }

            IsProcessing = true;
            StatusMessage = "Validation en cours...";
            AttemptCount = 0;
            FoundPassword = null;
            FoundSalt = null;
            ElapsedTime = TimeSpan.Zero;

            _stopwatch.Reset();
            _stopwatch.Start();
            _elapsedTimer.Start();

            _cancellationSource = new CancellationTokenSource();
            var token = _cancellationSource.Token;

            try
            {
                var result = await Task.Run(() => ValidateAgainstDictionary(HashToCrack, salt, DictionaryPath, token), token);

                AttemptCount = result.attempts;
                FoundPassword = result.success ? result.password : null;
                FoundSalt = result.success ? result.salt : null;

                StatusMessage = result.success
                    ? $"Mot de passe trouvé après {result.attempts} tentative(s)."
                    : "Aucune correspondance trouvée dans le dictionnaire.";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Validation annulée par l'utilisateur.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur : {ex.Message}";
            }
            finally
            {
                _elapsedTimer.Stop();
                _stopwatch.Stop();
                ElapsedTime = _stopwatch.Elapsed;

                _cancellationSource?.Dispose();
                _cancellationSource = null;
                IsProcessing = false;
            }
        }

        private (bool success, string? password, string salt, int attempts) ValidateAgainstDictionary(string targetHash, string salt, string dictionaryPath, CancellationToken token)
        {
            int attempts = 0;

            foreach (var rawLine in File.ReadLines(dictionaryPath))
            {
                token.ThrowIfCancellationRequested();

                var candidate = rawLine.Trim();
                if (string.IsNullOrEmpty(candidate))
                {
                    continue;
                }

                attempts++;
                ReportAttempt(attempts);

                var candidateHash = BCrypt.Net.BCrypt.HashPassword(candidate, salt);
                if (string.Equals(candidateHash, targetHash, StringComparison.Ordinal))
                {
                    return (true, candidate, salt, attempts);
                }
            }

            return (false, null, salt, attempts);
        }

        private void ReportAttempt(int attempts)
        {
            Application.Current.Dispatcher.Invoke(() => AttemptCount = attempts);
        }

        private void BrowseDictionary()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Sélectionnez le fichier dictionnaire",
                Filter = "Fichiers texte (*.txt)|*.txt|Tous les fichiers (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                DictionaryPath = dialog.FileName;
            }
        }

        private void CancelValidation()
        {
            if (_cancellationSource is null || !_cancellationSource.Token.CanBeCanceled)
            {
                return;
            }

            _cancellationSource.Cancel();
        }

        private async Task UpdateDictionaryMetadataAsync()
        {
            if (!File.Exists(DictionaryPath))
            {
                DictionaryCount = 0;
                return;
            }

            try
            {
                var count = await Task.Run(() => File.ReadLines(DictionaryPath)
                                                     .Select(line => line.Trim())
                                                     .Count(line => !string.IsNullOrEmpty(line)));
                DictionaryCount = count;

                if (!IsProcessing)
                {
                    StatusMessage = count > 0
                        ? $"Dictionnaire chargé ({count} mot(s))."
                        : "Le dictionnaire ne contient aucun mot valide.";
                }
            }
            catch (Exception ex)
            {
                DictionaryCount = 0;
                StatusMessage = $"Erreur de lecture du dictionnaire : {ex.Message}";
            }
        }

        private bool CanStartValidation() =>
            !IsProcessing &&
            !string.IsNullOrWhiteSpace(HashToCrack) &&
            File.Exists(DictionaryPath);

        private void RaiseCommandStates()
        {
            (BrowseDictionaryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (StartValidationCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (CancelValidationCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private static string? TryExtractSalt(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                return null;
            }

            if (!hash.StartsWith("$2", StringComparison.Ordinal))
            {
                return null;
            }

            return hash.Length >= 29 ? hash.Substring(0, 29) : null;
        }
    }
}
