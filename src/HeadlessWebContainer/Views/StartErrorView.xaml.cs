namespace HeadlessWebContainer.Views
{
    public partial class StartErrorView
    {
        public string ErrorMessage { get; }

        public StartErrorView(string errorMessage)
        {
            ErrorMessage = errorMessage;

            InitializeComponent();
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e) => Close();
    }
}
