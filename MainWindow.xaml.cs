using System;
using System.Windows;
using System.Windows.Threading;

namespace Password_reset
{
    public partial class MainWindow : Window
    {
        private PasswordManager _passwordManager;

        public MainWindow()
        {
            InitializeComponent();
            _passwordManager = new PasswordManager();
            InitializePlaceholders();
        }

        private void InitializePlaceholders()
        {
            PasswordTextBox.GotFocus += RemovePasswordPlaceholder;
            PasswordTextBox.LostFocus += ShowPasswordPlaceholder;

            ThreadCountTextBox.GotFocus += RemoveThreadCountPlaceholder;
            ThreadCountTextBox.LostFocus += ShowThreadCountPlaceholder;

            ShowPasswordPlaceholder(null, null);
            ShowThreadCountPlaceholder(null, null);
        }

        private void RemovePasswordPlaceholder(object sender, RoutedEventArgs e)
        {
            if (PasswordTextBox.Text == "")
            {
                PasswordPlaceholder.Visibility = Visibility.Hidden;
            }
        }

        private void ShowPasswordPlaceholder(object sender, RoutedEventArgs e)
        {
            if (PasswordTextBox.Text == "")
            {
                PasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void RemoveThreadCountPlaceholder(object sender, RoutedEventArgs e)
        {
            if (ThreadCountTextBox.Text == "")
            {
                ThreadCountPlaceholder.Visibility = Visibility.Hidden;
            }
        }

        private void ShowThreadCountPlaceholder(object sender, RoutedEventArgs e)
        {
            if (ThreadCountTextBox.Text == "")
            {
                ThreadCountPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void CreatePassword_Click(object sender, RoutedEventArgs e)
        {
            var password = PasswordTextBox.Text;
            _passwordManager.CreateAndStorePassword(password);
            MessageBox.Show("Password encrypted and stored successfully.");
        }

        private void StartAttack_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ThreadCountTextBox.Text, out int maxThreads))
            {
                var encryptedPassword = _passwordManager.GetEncryptedPassword();
                var attacker = new BruteForceAttacker(encryptedPassword, maxThreads);
                attacker.StartAttack((password, time) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ResultTextBlock.Text = password != null
                            ? $"Password found: {password} in {time.TotalSeconds} seconds."
                            : "Password not found.";
                    });
                });
            }
            else
            {
                MessageBox.Show("Invalid thread count.");
            }
        }
    }
}
