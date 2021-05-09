using MaSch.Core.Attributes;
using System;
using System.Windows;
using System.Windows.Input;
using MessageBox = MaSch.Presentation.Wpf.MessageBox;

#pragma warning disable SA1649 // File name should match first type name

namespace HeadlessWebContainer.Views
{
    [ObservablePropertyDefinition]
    internal interface IInputMessageBox_Props
    {
        public string? Message { get; set; }
        public string SelectedText { get; set; }
        public string SubmitButtonContent { get; set; }
    }

    [GenerateObservableObject]
    public partial class InputMessageBox : IInputMessageBox_Props
    {
        public Func<string, string?>? ValidationFunction { get; set; }

        public InputMessageBox()
        {
            _submitButtonContent = "OK";
            _selectedText = string.Empty;

            InitializeComponent();
        }

        private void SubmitButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedText))
            {
                MessageBox.Show(this, "Please type in a value.", "Headless Web Container", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var error = ValidationFunction?.Invoke(SelectedText);
            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(this, error, "Headless Web Container", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void InputTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsRepeat && e.Key == Key.Enter)
                SubmitButton_OnClick(sender, e);
        }
    }
}
