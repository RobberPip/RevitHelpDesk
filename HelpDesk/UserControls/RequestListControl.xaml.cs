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

    private async void RequestInfo(object sender, MouseButtonEventArgs e)
    {
        mainVM.Request = (RequestModel)RequestListBox.SelectedItem;
        await mainVM.LoadRequest();
    }
    private void ColumnHeader_Click(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

}
