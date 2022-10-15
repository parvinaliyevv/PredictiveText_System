namespace Solution.Views;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();

        DataContext = new MainViewModel();

        TextArea.Document = new FlowDocument((DataContext as MainViewModel)?.TextAreaValue);
    }    
}
