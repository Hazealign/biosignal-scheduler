using System.Windows;
using BiosignalScheduler.Export;

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
            scheduler.AddOperator(new WaveformExportV2());
            scheduler.AddOperator(new NumericExport());
            scheduler.Start();
        }
    }
}
