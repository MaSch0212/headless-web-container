namespace HeadlessWebContainer.Views
{
    public partial class StartErrorView
    {
        public string? ErrorMessage { get; set; }

        public StartErrorView()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e) => Close();
    }
}
