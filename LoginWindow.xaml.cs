using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Aviadispetcher
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        

        private int LogCheck()
        {
            int logUser = 0;
            if ((logTextBox.Text == "1") && (passwordTextBox.Text=="1") )
            {
                //Користувач
                //111
                logUser = 1;
            }
            else if ((logTextBox.Text == "2") && (passwordTextBox.Text == "2"))
            {
                //Редактор
                //222
                logUser = 2;
            }
            else
            {
                MessageBox.Show("Введіть правильний пароль!", "Помилка!");
            }
            return logUser;
        }
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Flight.logUser = LogCheck();
            if ((Flight.logUser==1) || (Flight.logUser==2))
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            this.Close();
        }
    }
}
