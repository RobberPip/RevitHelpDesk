using HelpDesk.UserControls;

namespace HelpDesk;
public partial class MainWindow : Window
{
    private MainVM mainVM;
    public MainWindow(UIApplication application)
    {
        InitializeComponent();
        mainVM = new MainVM(application,this);
        DataContext = mainVM;
        ((MainVM)DataContext).LoadWindow();  
    }
}
