namespace HelpDesk.UserControls;

/// <summary>
/// Логика взаимодействия для RequestListControl.xaml
/// </summary>
public partial class RequestListControl : UserControl
{
    public RequestListControl(MainVM mainVM)
    {
        InitializeComponent();
        this.mainVM = mainVM;
    }
    public MainVM mainVM { get; private set; }

    private void RequestInfo(object sender, MouseButtonEventArgs e)
    {
        mainVM.Request = (RequestModel)RequestListBox.SelectedItem;
        mainVM.LoadRequest();
    }
}
