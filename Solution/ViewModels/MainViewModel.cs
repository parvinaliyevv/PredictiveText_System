namespace Solution.ViewModels;

public class MainViewModel : DependencyObject
{
    private PrefixTree _dictionary { get; init; } = new PrefixTree();

    public List<Word> FrequentlyUsedWords { get; init; }
    public List<string> FilteredWords { get; set; }

    public object PredectiveTextButtonValue
    {
        get { return (object)GetValue(PredectiveTextButtonValueProperty); }
        set { SetValue(PredectiveTextButtonValueProperty, value); }
    }
    public static readonly DependencyProperty PredectiveTextButtonValueProperty =
        DependencyProperty.Register("PredectiveTextButtonValue", typeof(object), typeof(MainViewModel));
    public Paragraph TextAreaValue { get; set; }

    public string CurrentWord { get; set; }
    public List<Inline> WordLetters { get; set; }

    public int PredectiveTextIndex { get; set; }
    public char PreviousLetter { get; set; }

    public Stopwatch LetterChangeTimer { get; set; }

    public RelayCommand WriteWordCommand { get; set; }
    public RelayCommand AddWordCommand { get; set; }
    public RelayCommand WriteSymbolCommand { get; set; }
    public RelayCommand DeleteSymbolCommand { get; set; }
    public RelayCommand PrevWordCommand { get; set; }
    public RelayCommand NextWordCommand { get; set; }


    public MainViewModel()
    {
        FrequentlyUsedWords = new List<Word>();
        FilteredWords = new List<string>();

        PredectiveTextButtonValue = string.Empty;
        CurrentWord = string.Empty;

        PredectiveTextIndex = -1;
        PreviousLetter = ' ';

        WordLetters = new();
        LetterChangeTimer = new Stopwatch();

        TextAreaValue = new()
        {
            FontSize = 16,
            FontFamily = new FontFamily("Calibri"),
        };

        WriteWordCommand = new((sender) => WriteWord(sender), _ => !string.IsNullOrWhiteSpace(PredectiveTextButtonValue.ToString()));
        AddWordCommand = new((sender) => AddWord(sender), _ => !string.IsNullOrWhiteSpace(CurrentWord) && char.IsLetter(CurrentWord[0]));

        WriteSymbolCommand = new((sender) => WriteSymbol(sender));
        DeleteSymbolCommand = new((sender) => DeleteSymbol(sender), _ => TextAreaValue.Inlines.Count != 0);

        PrevWordCommand = new(_ =>
        {
            PredectiveTextButtonValue = FilteredWords[--PredectiveTextIndex];
            SoundService.PlaySoundFromThisPath(@"Assets\Sounds\KeyPress.wav");
        }, _ => FilteredWords.Count != 0 && PredectiveTextIndex > 0);
        NextWordCommand = new(_ =>
        {
            PredectiveTextButtonValue = FilteredWords[++PredectiveTextIndex];
            SoundService.PlaySoundFromThisPath(@"Assets\Sounds\KeyPress.wav");
        }, _ => FilteredWords.Count != 0 && PredectiveTextIndex < FilteredWords.Count - 1);

        var words = File.ReadAllLines(string.Format("{0}{1}", DirectoryService.GetProjectParentFolder(), @"Assets\Data\Dictionary.txt"));
        var timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(50) };

        for (int i = 0; i < words.Length; i++) _dictionary.Insert(words[i]);

        timer.Tick += (sender, e) => CommandManager.InvalidateRequerySuggested();
        timer.Start();
    }


    private void WriteSymbol(object? sender)
    {
        var button = sender as ButtonT9;
        string letter = string.Empty;

        ArgumentNullException.ThrowIfNull(button);

        LetterChangeTimer.Stop();

        if (button.IsLongPress)
        {
            if (button.TNumber == -1) return;

            letter = button.TNumber.ToString();
            
            button.IsLongPress = false;
        }
        else if (button.IsWrited)
        {
            button.IsWrited = false;
            return;
        }
        else if (button.TSymbols == string.Empty) return;
        else if(button.TNumber == 0)
        {
            CurrentWord = string.Empty;
            letter = " ";

            WordLetters.Clear();
        }
        else if (button.TSymbols.Contains(PreviousLetter) && LetterChangeTimer.ElapsedMilliseconds <= 600l)
        {
            if (button.TNumber == -1)
            {
                if (!string.IsNullOrWhiteSpace(button.TSymbols))
                    letter = button.TSymbols[0].ToString();
            }
            else
            {
                var index = button.TSymbols.IndexOf(PreviousLetter);

                if (index + 1 >= button.TSymbols.Length) index = -1;

                letter = Convert.ToString(button.TSymbols[index + 1]);

                CurrentWord = CurrentWord.Substring(0, CurrentWord.Length - 1);
                TextAreaValue.Inlines.Remove(TextAreaValue.Inlines.LastInline);
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(button.TSymbols))
                letter = button.TSymbols[0].ToString();
        }

        var inline = new Span(new Run(letter));

        SoundService.PlaySoundFromThisPath(@"Assets\Sounds\KeyPress.wav");

        Task.Factory.StartNew(() =>
        {
            var dispatcher = Application.Current.Dispatcher;

            PredectiveTextIndex = -1;
            PreviousLetter = char.Parse(letter);
            FilteredWords = new List<string>();

            if (letter != " " && !char.IsNumber(char.Parse(letter)))
            {
                CurrentWord += letter;
                WordLetters.Add(inline);

                FilteredWords.AddRange(FrequentlyUsedWords.FindAll(item => item.Text.Trim().StartsWith(CurrentWord)).Select(item => item.Text));
                FilteredWords.AddRange(_dictionary.ToList().FindAll(item => item.Trim().StartsWith(CurrentWord)));
            }

            if (FilteredWords.Count != 0) dispatcher.Invoke(() => PredectiveTextButtonValue = FilteredWords[++PredectiveTextIndex]);
            else dispatcher.Invoke(() => PredectiveTextButtonValue = string.Empty);
        });

        TextAreaValue.Inlines.Add(inline);

        if (letter != " " && !char.IsNumber(char.Parse(letter)))
        {
            LetterChangeTimer = new Stopwatch();
            LetterChangeTimer.Start();
        }
    }

    private void DeleteSymbol(object? sender)
    {
        SoundService.PlaySoundFromThisPath(@"Assets\Sounds\KeyPress.wav");

        TextAreaValue.Inlines.Remove(TextAreaValue.Inlines.LastInline);
        if (WordLetters.Count != 0) WordLetters.RemoveAt(WordLetters.Count - 1);

        FilteredWords.Clear();

        PreviousLetter = ' ';
        PredectiveTextIndex = -1;
        PredectiveTextButtonValue = string.Empty;

        if (CurrentWord == string.Empty || CurrentWord.Length == 1)
        {
            CurrentWord = string.Empty;
            return;
        }

        CurrentWord = CurrentWord.Substring(0, CurrentWord.Length - 1);

        Task.Factory.StartNew(() =>
        {
            var dispatcher = Application.Current.Dispatcher;

            FilteredWords.AddRange(FrequentlyUsedWords.FindAll(item => item.Text.Trim().StartsWith(CurrentWord)).Select(item => item.Text));
            FilteredWords.AddRange(_dictionary.ToList().FindAll(item => item.Trim().StartsWith(CurrentWord)));

            if (FilteredWords.Count != 0) dispatcher.Invoke(() =>
            {
                if (NextWordCommand.CanExecute(null)) NextWordCommand.Execute(null);
            });
        });
    }

    private void WriteWord(object? sender)
    {
        if (PredectiveTextButtonValue == string.Empty) return;
        
        SoundService.PlaySoundFromThisPath(@"Assets\Sounds\KeyPress.wav");

        CurrentWord = string.Empty;
        PredectiveTextIndex = -1;
        PreviousLetter = ' ';

        foreach (var item in WordLetters) TextAreaValue.Inlines.Remove(item);
        WordLetters.Clear();

        var data = PredectiveTextButtonValue.ToString().Substring(0);
        PredectiveTextButtonValue = string.Empty;

        foreach (var item in data)
        {
            var inline = new Span(new Run(item.ToString()));
            TextAreaValue.Inlines.Add(inline);
        }
        TextAreaValue.Inlines.Add(" ");

        FilteredWords.Clear();

        Task.Factory.StartNew(() =>
        {
            var word = FrequentlyUsedWords.Find(item => item.Equals(new Word(data)));

            if (word is null) FrequentlyUsedWords.Add(new Word(data));
            else word.Count++;

            FrequentlyUsedWords.Sort((firstElement, secondElement) =>
            {
                if (firstElement.Count < secondElement.Count) return -1;
                else if (firstElement.Count > secondElement.Count) return 1;
                else return 0;
            });
        });
    }

    private void AddWord(object? sender)
    {
        if (string.IsNullOrWhiteSpace(CurrentWord) || !char.IsLetter(CurrentWord[0])) return;

        var word = CurrentWord.Substring(0);

        if (_dictionary.Exists(ref word))
        {
            MessageBox.Show("The word is already in the dictionary!!", "Operation failed!", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Task.Factory.StartNew(() =>
        {
            _dictionary.Insert(CurrentWord);

            FrequentlyUsedWords.Add(new Word(CurrentWord));
            File.AppendAllTextAsync(string.Format("{0}{1}", DirectoryService.GetProjectParentFolder(), @"Assets\Data\Dictionary.txt"), string.Format("\n{0}", CurrentWord));

            MessageBox.Show("The word has been added to the dictionary!", "Operation successfully!", MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }
}
