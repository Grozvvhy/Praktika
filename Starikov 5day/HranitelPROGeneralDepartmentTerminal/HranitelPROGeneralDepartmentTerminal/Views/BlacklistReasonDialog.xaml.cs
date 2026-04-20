using System.Windows;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class BlacklistReasonDialog : Window
    {
        public string Reason { get; private set; }

        public BlacklistReasonDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReasonTextBox.Text))
            {
                MessageBox.Show("Укажите причину.");
                return;
            }
            Reason = ReasonTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}