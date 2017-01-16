using System.Windows;
using BiosignalScheduler.Export;
using BiosignalScheduler.Scheduler;

namespace BiosignalScheduler
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var scheduler = new Scheduler.Scheduler();
            scheduler.Start();
            scheduler.AddOperator(new WaveformExport());
            scheduler.AddOperator(new NumericExport());
        }
    }
}
