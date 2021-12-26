using DicewareGenerator.Extensions;
using DicewareGenerator.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

[assembly: InternalsVisibleTo("DicewareGeneratorTest")]

namespace DicewareGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Random _rnd;
        private string? _currentAppFolder;
        private string? _currentWordlistFolder;
        private int _diceCount;
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
                if (value != null)
                    ParseWordlist(LoadWordlist(value.Value.ToString()));
            }
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
            _rnd = new Random();
            SettingsLoad();
            WordSeparatorChar = " ";    // Part of a ugly hack because the space " " wont work well stored in the app settings.
            InitializeFilestructure();
        }

        #region WPF Buttons

        private void Button_GeneratePhrases_Clicked(object sender, RoutedEventArgs e)
        {
            if (_diceCount == 0)
                return;

            if (string.IsNullOrEmpty(WordSeparatorChar))
            {
                WordSeparatorChar = " ";
            }

            var sb = new StringBuilder();

            for (int i = 0; i < NumberOfPhrasesToGenerate; i++)
            {
                List<string> parts = new();
                bool state = false;
                while (!state)
                {
                    string? word = GetWordFromWordlist(DiceArrayToInt(RollDice(_diceCount)));

                    if (word != null)
                    {
                        parts.Add(word);
                        state = IsValidPassword(string.Join(WordSeparatorChar, parts));
                    }
                    else
                    {
                        throw new NullReferenceException("Null value found but not expected");
                    }
                }
                sb.AppendLine(string.Join(WordSeparatorChar, parts));
            }

            SettingsSave();
            GeneratedPasswords = sb.ToString();
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

        //internal void Button_SelectFile_Clicked(object sender, RoutedEventArgs e)
        //{
        //    var f = SelectFile();
        //    if (f == null)
        //        return;

        //    SelectedWordlistFile = f;

        //    // Make objects out of the wordlist
        //    var rows = LoadWordist(f);

        //    if (rows == null)
        //        return;

        //    _words = new List<DiceWordModel>();
        //    foreach (var row in rows)
        //    {
        //        var rowData = row.Trim().Split("\t");
        //        _words.Add(new DiceWordModel()
        //        {
        //            DiceValues = Int64.Parse(rowData[0]),
        //            Word = rowData[1].Trim(),
        //        });
        //    }

        //    // Look at the first item to identify how many dice we need to roll to select a workd from the wordlist.
        //    _diceCount = _words[0].DiceValues.ToString().Length;

        private bool CanGeneratePhrasesValidation()
        {
            if ((MinWords > 0 || MinCharacters > 0) && (NumberOfPhrasesToGenerate > 0))
                return true;
            return false;
        }

        //    // Store the data into settings for later use.
        //    Settings.Default.LoadedWordList = string.Join(Environment.NewLine, rows);
        //    SettingsSave();
        //}
        private void ParseWordlist(List<string>? rowdata)
        {
            if (rowdata == null)
                throw new ArgumentNullException();

            var words = new List<DiceWordModel>();
            foreach (var row in rowdata)
            {
                var rowData = row.Trim().Split("\t");
                words.Add(new DiceWordModel()
                {
                    DiceValues = Int64.Parse(rowData[0]),
                    Word = rowData[1].Trim(),
                });
            }

            if (words.Count > 0)
            {
                LoadedWords = words;

                // Look at the first item to identify how many dice we need to roll to select a workd from the wordlist.
                _diceCount = LoadedWords[0].DiceCount;
            }
        }

        #endregion WPF Buttons

        internal static Int64 DiceArrayToInt(int[] numbers)
        {
            Int64 result = 0;
            for (int i = 0; i < numbers.Length; i++)
            {
                Int64 multiplier = (Int64)Math.Pow(10, numbers.Length - 1 - i);
                result += numbers[i] * multiplier;
            }

            return result;
        }

        internal int[] RollDice(int dices)
        {
            var results = new List<int>();
            for (int i = 0; i < dices; i++) results.Add(RollOneDice());

            return results.ToArray();
        }

        internal int RollOneDice()
        {
            return _rnd.Next(1, 7);
        }

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

        private static bool CheckIfLineIsIgnorable(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return true;
            if (string.IsNullOrEmpty(text)) return true;
            if (text.Trim().Length == 0) return true;
            if (text.Trim()[0..1] == "#") return true;
            return false;
        }

        private static List<string>? LoadWordlist(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException($"Cannot find file {file}");

            var rows = new List<string>();
            try
            {
                using (var sr = new StreamReader(file))
                {
                    while (sr.Peek() >= 0)
                    {
                        var x = sr.ReadLine();
                        if (x != null && !CheckIfLineIsIgnorable(x))
                            rows.Add(x.Trim());
                    }
                    return rows;
                }
            }
            catch (Exception)
            {
                throw new FileLoadException($"Failed to read file {file}");
            }
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

        private string? GetWordFromWordlist(Int64 value)
        {
            return LoadedWords?.Where(x => x.DiceValues == value).Select(x => x.Word).SingleOrDefault(); ;
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

        private bool IsValidPassword(string password)
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