namespace HelpDesk.UserControls;

/// <summary>
/// Логика взаимодействия для RequestInfoControl.xaml
/// </summary>
public partial class RequestInfoControl : UserControl
{
    public RequestInfoControl()
    {
        InitializeComponent();
    }
    private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            int caretIndex = textBox.CaretIndex;
            textBox.Text = textBox.Text.Insert(caretIndex, Environment.NewLine);
            textBox.CaretIndex = caretIndex + Environment.NewLine.Length;
            e.Handled = true;
        }
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SendButton.IsEnabled=MessageBox.Text.Length > 0?true:false;
    }
}
