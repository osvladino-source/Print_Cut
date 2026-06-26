using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using LaserPrintCutAddin.Models;
using LaserPrintCutAddin.Services;
using LaserPrintCutAddin.Views;

using WpfWindow = System.Windows.Window;

namespace LaserPrintCutAddin.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ImageProcessingService _imageProcessingService;
        private readonly CoreldrawIntegrationService _coreldrawService;

        private Mat _previewImage;
        private BitmapSource _previewBitmapSource;
        private bool _isProcessing;
        private string _selectedCorner = "Sharp (Live)";
        private double _offsetValue = 0.5;
        private int _thresholdValue = 128;
        private bool _keepHoles = false;
        private WpfWindow _ownerWindow;

        public event PropertyChangedEventHandler PropertyChanged;

        public Mat PreviewImage
        {
            get => _previewImage;
            set { _previewImage = value; OnPropertyChanged(); }
        }

        public BitmapSource PreviewBitmapSource
        {
            get => _previewBitmapSource;
            set { _previewBitmapSource = value; OnPropertyChanged(); }
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set { _isProcessing = value; OnPropertyChanged(); }
        }

        public bool BuildContour => true;
        public bool BuildBoxes => false;
        public string ContourType => "Cutting Contour";

        public List<string> CornerTypes { get; } = new List<string>
        {
            "Round (volta/arched)",
            "Sharp (Live)",
            "Bevel (Chamfered)"
        };

        public string SelectedCorner
        {
            get => _selectedCorner;
            set { _selectedCorner = value; OnPropertyChanged(); }
        }

        public double OffsetValue
        {
            get => _offsetValue;
            set { _offsetValue = value; OnPropertyChanged(); }
        }

        public int ThresholdValue
        {
            get => _thresholdValue;
            set { _thresholdValue = value; OnPropertyChanged(); }
        }

        public bool KeepHoles
        {
            get => _keepHoles;
            set { _keepHoles = value; OnPropertyChanged(); }
        }

        public bool OnlyContour => true;
        public bool SeparatePrintCut => false;

        public ICommand AcceptCommand { get; }
        public ICommand CancelCommand { get; }

        public MainViewModel(dynamic corelApp = null)
        {
            _imageProcessingService = new ImageProcessingService();
            _coreldrawService = new CoreldrawIntegrationService(corelApp);

            AcceptCommand = new RelayCommand(OnAcceptClick);
            CancelCommand = new RelayCommand(OnCancel);
        }

        public void SetOwner(WpfWindow owner)
        {
            _ownerWindow = owner;
        }

        public void LoadImageFromCorelDRAW()
        {
            try
            {
                var imagePath = _coreldrawService.GetSelectedImagePath();
                var mat = new Mat(imagePath);
                PreviewImage = mat;
                PreviewBitmapSource = MatToBitmapSource(mat);
                File.Delete(imagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(_ownerWindow,
                    $"Nao foi possivel carregar a imagem do CorelDRAW:\n{ex.Message}\n\n" +
                    "Selecione um objeto no CorelDRAW e tente novamente.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void OnAcceptClick()
        {
            try
            {
                IsProcessing = true;

                if (PreviewImage == null || PreviewImage.Empty())
                {
                    MessageBox.Show(_ownerWindow,
                        "Nenhuma imagem carregada. Selecione um objeto no CorelDRAW primeiro.",
                        "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var processedResult = await _imageProcessingService.ProcessImageAsync(
                    PreviewImage, (byte)ThresholdValue, OffsetValue, KeepHoles);

                if (processedResult.ContourPoints == null || processedResult.ContourPoints.Length == 0)
                {
                    MessageBox.Show(_ownerWindow,
                        "Nenhum contorno encontrado. Ajuste o Threshold e tente novamente.",
                        "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (OnlyContour)
                {
                    var result = _coreldrawService.CreateOnlyContour(
                        processedResult.ContourPoints,
                        processedResult.ApproximatedContours);
                    if (!result.Success)
                        throw new Exception(result.Message);
                }
                else
                {
                    var bitmap = MatToBitmap(PreviewImage);
                    var result = _coreldrawService.CreatePrintCutMode(
                        bitmap,
                        processedResult.ContourPoints,
                        processedResult.ApproximatedContours);
                    bitmap.Dispose();
                    if (!result.Success)
                        throw new Exception(result.Message);
                }

                MessageBox.Show(_ownerWindow, "Operacao concluida com sucesso!",
                    "Laser Print & Cut", MessageBoxButton.OK, MessageBoxImage.Information);

                _ownerWindow?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_ownerWindow,
                    $"Erro: {ex.Message}",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void OnCancel()
        {
            _ownerWindow?.Close();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static Bitmap MatToBitmap(Mat mat)
        {
            var tempFile = Path.GetTempFileName() + ".bmp";
            Cv2.ImWrite(tempFile, mat);
            var image = new Bitmap(tempFile);
            File.Delete(tempFile);
            return image;
        }

        private static BitmapSource MatToBitmapSource(Mat mat)
        {
            var tempFile = Path.GetTempFileName() + ".bmp";
            try
            {
                Cv2.ImWrite(tempFile, mat);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(tempFile);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}