using HelpDesk.UserControls;

namespace HelpDesk;
public partial class MainWindow : Window
{
    private MainVM mainVM;
<<<<<<< HEAD
    public MainWindow()
    {
        InitializeComponent();
        mainVM = new MainVM();
        DataContext = mainVM;
        ((MainVM)DataContext).LoadWindow();
    }
    public MainWindow(UIApplication application)
    {
        InitializeComponent();
        mainVM = new MainVM(application);
        DataContext = mainVM;
        ((MainVM)DataContext).LoadWindow();
=======
    public MainWindow(UIApplication application)
    {
        InitializeComponent();
        mainVM = new MainVM(application,this);
        DataContext = mainVM;
        ((MainVM)DataContext).LoadWindow();  
>>>>>>> 4c2db5e (first version)
    }
}
