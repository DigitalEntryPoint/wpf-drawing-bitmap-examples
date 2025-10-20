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

    private void LoadImage_Click(object sender, RoutedEventArgs e)
    {
        //new kimi paint window
        KimiPaintWindow kimiPaintWindow = new KimiPaintWindow();
        kimiPaintWindow.Owner = this;
        kimiPaintWindow.Show();
    }

    private void LoopWindowTest1_Click(object sender, RoutedEventArgs e)
    {
        //new loop window test 1
        LoopWindowTest1 loopWindowTest1 = new LoopWindowTest1();
        loopWindowTest1.Owner = this;
        loopWindowTest1.Show();
    }

    private void LoopWindowTest2_Click(object sender, RoutedEventArgs e)
    {
        //new loop window test 2
        LoopWindowTest2 loopWindowTest2 = new LoopWindowTest2();
        loopWindowTest2.Owner = this;
        loopWindowTest2.Show();
    }
}