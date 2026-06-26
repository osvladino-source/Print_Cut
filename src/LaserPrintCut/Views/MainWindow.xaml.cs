using System.Windows;
using System.Windows.Controls;
using LaserPrintCutAddin.ViewModels;

namespace LaserPrintCutAddin.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SetOwner(this);
                vm.LoadImageFromCorelDRAW();
            }
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.AcceptCommand.CanExecute(null))
            {
                vm.AcceptCommand.Execute(null);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.CancelCommand.CanExecute(null))
            {
                vm.CancelCommand.Execute(null);
            }
        }

        private void ThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (tbThresholdValue != null)
                tbThresholdValue.Text = ((int)e.NewValue).ToString();
        }
    }
}