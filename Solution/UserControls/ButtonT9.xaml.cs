namespace Solution.UserControls;

public partial class ButtonT9 : UserControl
{
    public int TNumber
    {
        get { return (int)GetValue(TNumberProperty); }
        set { SetValue(TNumberProperty, value); }
    }
    public static readonly DependencyProperty TNumberProperty =
        DependencyProperty.Register("TNumber", typeof(int), typeof(ButtonT9));

    public string TSymbols
    {
        get { return (string)GetValue(TSymbolsProperty); }
        set { SetValue(TSymbolsProperty, value); }
    }
    public static readonly DependencyProperty TSymbolsProperty =
        DependencyProperty.Register("TSymbols", typeof(string), typeof(ButtonT9));

    public RelayCommand Command
    {
        get { return (RelayCommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register("Command", typeof(RelayCommand), typeof(ButtonT9));

    public object CommandParameter
    {
        get { return (object)GetValue(CommandParameterProperty); }
        set { SetValue(CommandParameterProperty, value); }
    }
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register("CommandParameter", typeof(object), typeof(ButtonT9));

    public DispatcherTimer LongPressTimer { get; set; }

    public bool IsLongPress { get; set; } = false;
    public bool IsWrited { get; set; } = false;


    public ButtonT9()
    {
        InitializeComponent();

        DataContext = this;

        LongPressTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(400) };
        LongPressTimer.Tick += (_, _) => { IsLongPress = true; IsWrited = true; IsEnabled = false; PreviewMouseUp_ButtonClicked(this, null); };
    }


    private void PreviewMouseDown_ButtonClicked(object sender, MouseButtonEventArgs e)
        => LongPressTimer.Start();

    private void PreviewMouseUp_ButtonClicked(object sender, MouseButtonEventArgs e)
    {
        LongPressTimer.Stop();

        if (IsWrited) Command.Execute(this);
        IsEnabled = true;
    }
}
