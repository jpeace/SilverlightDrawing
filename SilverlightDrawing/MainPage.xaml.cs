using System.Windows.Controls;
using System.Windows.Media;

namespace SilverlightDrawing
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();

            main.Background = new SolidColorBrush(Colors.Gray);
            main.Refresh();
        }
    }
}
