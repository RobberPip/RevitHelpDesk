namespace HelpDesk.UserControls
{
    /// <summary>
    /// Логика взаимодействия для FileDetailControl.xaml
    /// </summary>
    public partial class FileDetailControl : UserControl
    {
        public FileDetailControl()
        {
            InitializeComponent();
        }
        public FileDetailModel FileDetail
        {
            get { return (FileDetailModel)GetValue(FileDetailProperty); }
            set { SetValue(FileDetailProperty, value); }
        }

        public static readonly DependencyProperty FileDetailProperty =
            DependencyProperty.Register("FileDetail", typeof(FileDetailModel), typeof(FileDetailControl));

    }
}
