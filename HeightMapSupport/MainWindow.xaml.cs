// © 2022 Jong-il Hong

using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace Haruby.Uesk.HeightMapSupport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty SourceFilePathProperty = DependencyProperty.Register(
            nameof(SourceFilePath), typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty OutputFilePathProperty = DependencyProperty.Register(
            nameof(OutputFilePath), typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SourceHeightMetersProperty = DependencyProperty.Register(
            nameof(SourceHeightMeters), typeof(double), typeof(MainWindow), new PropertyMetadata(100d));
        public static readonly DependencyProperty OffsetMetersProperty = DependencyProperty.Register(
            nameof(OffsetMeters), typeof(double), typeof(MainWindow), new PropertyMetadata(0d));
        public static readonly DependencyProperty IsBaseAsZeroProperty = DependencyProperty.Register(
            nameof(IsBaseAsZero), typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

        public string SourceFilePath { get => (string)GetValue(SourceFilePathProperty); set => SetValue(SourceFilePathProperty, value); }
        public string OutputFilePath { get => (string)GetValue(OutputFilePathProperty); set => SetValue(OutputFilePathProperty, value); }

        public double SourceHeightMeters { get => (double)GetValue(SourceHeightMetersProperty); set => SetValue(SourceHeightMetersProperty, value); }
        public double OffsetMeters { get => (double)GetValue(OffsetMetersProperty); set => SetValue(OffsetMetersProperty, value); }
        public bool IsBaseAsZero { get => (bool)GetValue(IsBaseAsZeroProperty); set => SetValue(IsBaseAsZeroProperty, value); }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenSourceFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new() { Filter = "PNG files|*.png|All files|*.*", };
            if (dialog.ShowDialog() is not true)
            {
                return;
            }
            SourceFilePath = dialog.FileName;
        }

        private void SaveOutputFileButton_Click(object sender, RoutedEventArgs e)
        {
            string srcFilePath = SourceFilePath;
            string outputFileName = string.Empty;
            if (!string.IsNullOrEmpty(srcFilePath))
            {
                string srcFilename = Path.GetFileNameWithoutExtension(srcFilePath);
                string srcExt = Path.GetExtension(srcFilePath);
                outputFileName = srcFilename + "_modified" + srcExt;
            }
            SaveFileDialog dialog = new() { Filter = "All files|*.*", FileName = outputFileName, };
            if (dialog.ShowDialog() is not true)
            {
                return;
            }
            OutputFilePath = dialog.FileName;
        }

        private void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            string sourceFilePath = SourceFilePath;
            string outputFilePath = OutputFilePath;
            if (string.IsNullOrEmpty(sourceFilePath))
            {
                MessageBox.Show("Please select source file first.", "Modify", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (string.IsNullOrEmpty(outputFilePath))
            {
                MessageBox.Show("Please select output file first.", "Modify", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            double sourceHeightMeters = SourceHeightMeters;
            double offsetMeters = OffsetMeters;
            bool isBaseAsZero = IsBaseAsZero;
            try
            {
                HeightMapUtil.Modify(sourceFilePath, outputFilePath, sourceHeightMeters, offsetMeters, isBaseAsZero);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error occured.\n\n" + ex, "Modify", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Done.", "Modify", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
