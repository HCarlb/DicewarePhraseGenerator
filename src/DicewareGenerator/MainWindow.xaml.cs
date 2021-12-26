using DicewareGenerator.Extensions;
using DicewareGenerator.Models;
using DicewareGenerator.Properties;
using DicewareGenerator.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

[assembly: InternalsVisibleTo("DicewareGeneratorTest")]

namespace DicewareGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //private readonly Random _rnd;
        private string? _currentAppFolder;

        private string? _currentWordlistFolder;

        //private int _diceCount;
        private string? _generatedPasswords;

        private List<DiceWordModel>? _loadedWords;
        private int _minCharacters;
        private int _minNumeric;
        private int _minSpecial;
        private int _minUppercase;
        private int _minWords;
        private int _numberOfPhrasesToGenerate;
        private ComboBoxItem? _selectedComboBoxItem;
        private string? _selectedWordlistFile;
        private List<ComboBoxItem>? _wordlistItemsSource;
        private string? _wordSeparatorChar;
        private readonly DiceService _diceService;
        private readonly WordlistReader _wordistReaderService;

        public bool CanGeneratePhrases => CanGeneratePhrasesValidation();

        public string? GeneratedPasswords
        {
            get { return _generatedPasswords; }
            set
            {
                _generatedPasswords = value;
                OnPropertyChanged(nameof(GeneratedPasswords));
            }
        }

        public List<DiceWordModel>? LoadedWords
        {
            get { return _loadedWords; }
            set
            {
                _loadedWords = value;
                OnPropertyChanged(nameof(LoadedWords));
            }
        }

        public int MinCharacters
        {
            get { return _minCharacters; }
            set
            {
                _minCharacters = value;
                OnPropertyChanged(nameof(MinCharacters));
                OnPropertyChanged(nameof(CanGeneratePhrases));
            }
        }

        public int MinNumeric
        {
            get { return _minNumeric; }
            set
            {
                _minNumeric = value;
                OnPropertyChanged(nameof(MinNumeric));
            }
        }

        public int MinSpecial
        {
            get { return _minSpecial; }
            set
            {
                _minSpecial = value;
                OnPropertyChanged(nameof(MinSpecial));
            }
        }

        public int MinUppercase
        {
            get { return _minUppercase; }
            set
            {
                _minUppercase = value;
                OnPropertyChanged(nameof(MinUppercase));
            }
        }

        public int MinWords
        {
            get { return _minWords; }
            set
            {
                _minWords = value;
                OnPropertyChanged(nameof(MinWords));
                OnPropertyChanged(nameof(CanGeneratePhrases));
            }
        }

        public int NumberOfPhrasesToGenerate
        {
            get { return _numberOfPhrasesToGenerate; }
            set
            {
                _numberOfPhrasesToGenerate = value;
                OnPropertyChanged(nameof(NumberOfPhrasesToGenerate));
                OnPropertyChanged(nameof(CanGeneratePhrases));
            }
        }

        public ComboBoxItem? SelectedWordlist
        {
            get { return _selectedComboBoxItem; }
            set
            {
                _selectedComboBoxItem = value;
                OnPropertyChanged(nameof(SelectedWordlist));

                // Selection changed load a new wordlist
                if (value != null) LoadNewWordlistAsync(value);
            }
        }

        private async void LoadNewWordlistAsync(ComboBoxItem? comboboxItem)
        {
            var wordList = await Task.Run(() => WordlistReader.LoadWordlist(comboboxItem.Value.ToString()));
            var diceWordList = await Task.Run(() => WordlistReader.ParseWordlist(wordList));

            // Update the loadedwords so the UI is updated
            LoadedWords = diceWordList;
        }

        public string? SelectedWordlistFile
        {
            get { return _selectedWordlistFile; }
            set
            {
                _selectedWordlistFile = value;
                OnPropertyChanged(nameof(SelectedWordlistFile));
            }
        }

        public List<ComboBoxItem>? WordlistItemsSource
        {
            get { return _wordlistItemsSource; }
            set
            {
                _wordlistItemsSource = value;
                OnPropertyChanged(nameof(WordlistItemsSource));
            }
        }

        public string? WordSeparatorChar
        {
            get { return _wordSeparatorChar; }
            set
            {
                _wordSeparatorChar = value;
                OnPropertyChanged(nameof(WordSeparatorChar));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            _diceService = new();
            _wordistReaderService = new();
            SettingsLoad();
            WordSeparatorChar = " ";    // Part of a ugly hack because the space " " wont work well stored in the app settings.
            InitializeFilestructure();
        }

        #region WPF Buttons

        private void Button_GeneratePhrases_Clicked(object sender, RoutedEventArgs e)
        {
            if (LoadedWords == null || LoadedWords.Count == 0)
            {
                return;
            }

            // Look at the loeded worldist and learn how many dice that is used to generate words from this wordlist.
            var diceCount = LoadedWords[0].DiceCount;

            // Check so the user at lease have a character as separator. If not set it to a "space".
            if (string.IsNullOrEmpty(WordSeparatorChar))
            {
                WordSeparatorChar = " ";
            }

            // Create a stringbuilde to store all the generated phrases
            var generatedPhrases = new StringBuilder();

            for (int i = 0; i < NumberOfPhrasesToGenerate; i++)
            {
                var phraseList = new List<string>();
                var phraseIsValid = false;

                while (!phraseIsValid)
                {
                    var diceValue = _diceService.GetDiceResult(diceCount);
                    var word = GetWordFromWordlist(diceValue);

                    if (word != null)
                    {
                        phraseList.Add(word);

                        // Test the phrase so it contain minimum requirements specified by user
                        phraseIsValid = IsPhraseValid(string.Join(WordSeparatorChar, phraseList));
                    }
                }
                generatedPhrases.AppendLine(string.Join(WordSeparatorChar, phraseList));
            }

            // Store settings to settings
            SettingsSave();

            // Display the passphrases to the user
            GeneratedPasswords = generatedPhrases.ToString();
        }

        private void Button_OpenWordlistFolder_Clicked(object sender, RoutedEventArgs e)
        {
            if (_currentWordlistFolder == null)
                return;

            if (!Directory.Exists(_currentWordlistFolder))
                Directory.CreateDirectory(_currentWordlistFolder);

            if (Directory.Exists(_currentWordlistFolder))
                Process.Start("explorer.exe", @_currentWordlistFolder);
        }

        #endregion WPF Buttons

        #region WPF Form Validation

        private bool CanGeneratePhrasesValidation()
        {
            if ((MinWords > 0 || MinCharacters > 0) && (NumberOfPhrasesToGenerate > 0))
                return true;
            return false;
        }

        #endregion WPF Form Validation

        internal void SettingsLoad()
        {
            var s = Settings.Default;
            MinWords = s.MinWords;
            MinCharacters = s.MinCharacters;
            MinNumeric = s.MinNumeric;
            MinSpecial = s.MinSpecial;
            MinUppercase = s.MinUppercase;
            //WordSeparatorChar = s.WordSeparator[1..1];    // Comment out because the space " " wont work well stored in the app settings.
            NumberOfPhrasesToGenerate = s.NumberOfPhrasesToGenerate;
        }

        internal void SettingsSave()
        {
            var s = Settings.Default;
            s.MinWords = MinWords;
            s.MinCharacters = MinCharacters;
            s.MinNumeric = MinNumeric;
            s.MinSpecial = MinSpecial;
            s.MinUppercase = MinUppercase;
            //s.WordSeparator = "\"" + WordSeparatorChar + "\"";    // Comment out because the space " " wont work well stored in the app settings.
            s.NumberOfPhrasesToGenerate = NumberOfPhrasesToGenerate;
            s.Save();
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private static string? SelectFile()
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "Text files (*.txt)|*.txt",
                Multiselect = false,
            };

            if (ofd.ShowDialog() == true)
            {
                return ofd.FileName;
            }
            return null;
        }

        private string? GetWordFromWordlist(long value)
        {
            return LoadedWords?.Where(x => x.DiceValues == value).Select(x => x.Word).SingleOrDefault();
        }

        private void InitializeFilestructure()
        {
            _currentAppFolder = Directory.GetCurrentDirectory();

            if (Directory.Exists(_currentAppFolder))
            {
                _currentWordlistFolder = Path.Combine(_currentAppFolder, Settings.Default.DefaultWordlistsPath);
                if (!Directory.Exists(_currentWordlistFolder))
                {
                    Directory.CreateDirectory(_currentWordlistFolder);
                }
                else
                {
                    var items = new List<ComboBoxItem>();
                    string[]? filesInFolder = Directory.GetFiles(_currentWordlistFolder);
                    foreach (string file in filesInFolder)
                    {
                        if (Path.GetExtension(file) == Settings.Default.WordlistExtension)
                        {
                            items.Add(
                                new ComboBoxItem
                                {
                                    Key = Path.GetFileNameWithoutExtension(file),
                                    Value = file,
                                }
                                );
                        }
                    }

                    WordlistItemsSource = items.OrderBy(x => x.Key).ToList();
                    if (WordlistItemsSource != null)
                    {
                        SelectedWordlist = WordlistItemsSource.First();
                    }
                }
            }
        }

        private bool IsPhraseValid(string password)
        {
            if (
                password.Length >= MinCharacters &&
                password.WordCount(WordSeparatorChar) >= MinWords
                )
            {
                return true;
            }
            return false;
        }
    }
}