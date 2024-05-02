using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HelpDesk.UserControls
{
    /// <summary>
    /// Логика взаимодействия для RequestAddControl.xaml
    /// </summary>
    public partial class RequestAddControl : UserControl
    {
        public RequestAddControl()
        {
            var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var directory = System.IO.Path.GetDirectoryName(executingAssembly.Location);
            var dllPath = System.IO.Path.Combine(directory, "Sdl.MultiSelectComboBox.dll");
            Assembly.LoadFrom(dllPath);

            InitializeComponent();
        }
    }
}
