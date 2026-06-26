using System;
using System.Runtime.InteropServices;
using System.Windows;
using LaserPrintCutAddin.ViewModels;
using LaserPrintCutAddin.Views;

namespace LaserPrintCutAddin
{
    [ComVisible(true)]
    [Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890")]
    [ProgId("LaserPrintCut.Addin")]
    public class Addin
    {
        public void Startup()
        {
        }

        public void ShowMainWindow()
        {
            try
            {
                var vm = new MainViewModel();
                var window = new MainWindow
                {
                    DataContext = vm
                };
                vm.SetOwner(window);
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro: {ex.Message}\n\n{ex.StackTrace}",
                    "Laser Print & Cut - Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void Shutdown()
        {
        }
    }
}
