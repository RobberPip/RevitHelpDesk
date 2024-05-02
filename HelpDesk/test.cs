using static System.Net.Mime.MediaTypeNames;

namespace HelpDesk;
public class Test
{
    [STAThread] // Указываем, что приложение должно быть запущено в STA-потоке
    static void Main(string[] args)
    {
        System.Windows.Application app = new System.Windows.Application();
        MainWindow window = new MainWindow();
        app.Run(window);
    }
}
