namespace HelpDesk.UserControls
{
    /// <summary>
    /// Логика взаимодействия для RequestAddControl.xaml
    /// </summary>
    public partial class RequestAddControl : UserControl
    {
        private MainVM mainVM {  get; set; }
        public RequestAddControl(MainVM mainVM)
        {
            var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var directory = System.IO.Path.GetDirectoryName(executingAssembly.Location);
            var dllPath = System.IO.Path.Combine(directory, "Sdl.MultiSelectComboBox.dll");
            Assembly.LoadFrom(dllPath);
            this.mainVM = mainVM;
            InitializeComponent();
        }

        private void Theme_TextChanged(object sender, TextChangedEventArgs e)
        {
           mainVM.Apply = Theme.Text.Length > 0 ? true : false;
        }
    }
}
