using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BasicBitmapManipulation.Windows;

namespace BasicBitmapManipulation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void BaseButton_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is Button button && button.Tag is string windowType)
        {
            Window? newWindow = windowType switch
            {
                "KimiPaint" => new KimiPaintWindow(),
                "LoopWindowTest1" => new LoopWindowTest1(),
                "LoopWindowTest2" => new LoopWindowTest2(),
                "LoopWindowTest3" => new LoopWindowTest3(),
                "LoopWindowTest4" => new LoopWindowTest4(),
                "RenderOutput" => new RenderOutputWindow(),
                _ => null
            };

            if (newWindow != null)
            {
                newWindow.Owner = this;
                newWindow.Show();
            }
        }
    }
}