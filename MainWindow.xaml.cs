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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data;
using MySql.Data.MySqlClient;
using Word = Microsoft.Office.Interop.Word;
using System.Reflection;
using System.IO;

namespace Aviadispetcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int flightNum;
        bool flightAdd = false;
        int flightCount;
        string connStr = "Server = remotemysql.com; Database = KursJzji3q; Uid = KursJzji3q; Pwd = Gt0diQIFHT;";
        string filePath = Environment.CurrentDirectory.ToString();
        Microsoft.Office.Interop.Word.Application wordApp;
        Microsoft.Office.Interop.Word.Document wordDoc;
        string sqlCommandCache = " ";

        public List<Flight> fList = new List<Flight>(85);
        public List<Flight> selectedCityList = new List<Flight>();
        public List<Flight> selectedCityTimeList = new List<Flight>();
        string xP;
        TimeSpan yP;
        

        public void OpenDbFile()
        {
            try
            {
               string connStr = "Server = remotemysql.com; Database = KursJzji3q; Uid = KursJzji3q; Pwd = Gt0diQIFHT;";
               MySqlConnection conn = new MySqlConnection(connStr);
        
                MySqlCommand command = new MySqlCommand();

                string commandString = "SELECT * FROM rozklad;";
                command.CommandText = commandString;
                command.Connection = conn;

                MySqlDataReader reader;
                command.Connection.Open();
                reader = command.ExecuteReader();

                int i = 0;
                while (reader.Read())
                {
                    fList.Add(new Flight((string)reader["number"], (string)reader["city"], (System.TimeSpan)reader["depature_time"],
                        (int)reader["free_seats"]));
                    i += 1;
                }
                reader.Close();
                command.Connection.Close();
                FlightListDG.ItemsSource = fList;
                if(groupBox2.Visibility == Visibility.Visible)
                {
                    Button1_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + char.ConvertFromUtf32(13) + char.ConvertFromUtf32(13) +
                    "Для завантаження файлу " + "виконайте команду Файл - Завантажити", "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadDataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FlightListDG.ItemsSource = null;
                fList.Clear();
                OpenDbFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + char.ConvertFromUtf32(13), "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void InfoFlightForm_Loaded(object sender, RoutedEventArgs e)
        {
            OpenDbFile();

            if (Aviadispetcher.Flight.logUser == 1)
            {
                mainMenu.Items.Remove(mainMenu.Items[1]);
                groupBox3.Visibility = Visibility.Hidden;
                this.Width = 820;
                this.Height = 400;
            }
            else if (Aviadispetcher.Flight.logUser == 2)
            {
                mainMenu.Items.Remove(mainMenu.Items[2]);
                groupBox1.Visibility = Visibility.Hidden;
                groupBox2.Visibility = Visibility.Hidden;
                Button3.Visibility = Visibility.Hidden;
                this.Width = 400;
                this.Height = 630;
            }
            FillCityList();
        }

        private void FlightListDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Aviadispetcher.Flight currFlight = new Aviadispetcher.Flight();
            currFlight = fList[FlightListDG.SelectedIndex];
            cityFlightTextBox.Text = currFlight.City;

            if (currFlight.Depature_time.Ticks == 4)
            {
                timeFlightTextBox.Text = "0" + currFlight.Depature_time;
            }
            else
            {
                timeFlightTextBox.Text = "" + currFlight.Depature_time;
            }
            numFlightTextBox.Text = currFlight.Number;
            freeSeatsTextBox.Text = Convert.ToString(currFlight.Free_seats);
            flightNum = FlightListDG.SelectedIndex;
        }

        private List<Flight> SelectX(string cityX)
        {
        
            List<Flight> selectedList = new List<Flight>();

            foreach (Flight it in fList)
            {
                if(it.City == cityX)
                {
                    selectedList.Add(it);
                }
            }
            return selectedList;
        }
        
        private List<Flight> SelectXY(TimeSpan DeadLine) 
        {
            List<Flight> selectedList = new List<Flight>();
            foreach (Flight it in selectedCityList)
                if (TimeSpan.Compare(it.Depature_time, DeadLine) >= 0)
                {
                    selectedList.Add(it);
                }
                
            return selectedList;
         }

        private void FillCityList(){
            bool nameExist = false;
            cityList.Items.Clear();
            cityList.Items.Add(fList[0].City);

            for (int i = 1; i < fList.Count; i++)
            {
                for (int j = 0; j < cityList.Items.Count; j++)
                {
                    if (cityList.Items[j].ToString() == fList[i].City)
                    {
                        nameExist = true;
                    }
                }

                if (!nameExist)
                {
                    cityList.Items.Add(fList[i].City);
                }

                nameExist = false;
            }
        }
        private void MenuItem2_Click(object sender, RoutedEventArgs e)// (+/-)
        {
        
            FillCityList();
            selectXList.Items.Clear();
        }

        private void MenuItem5_Click(object sender, RoutedEventArgs e)
        {
            flightAdd = false;
        }

    /*    private void ChangeFlightListData(int num)
        {
            fList[num].City = cityFlightTextBox.Text;
            fList[num].Number = numFlightTextBox.Text;
            fList[num].Depature_time = TimeSpan.Parse(timeFlightTextBox.Text);
            fList[num].Free_seats = Int32.Parse(freeSeatsTextBox.Text);

            if (flightAdd)
            {
                flightCount = flightCount + 1;
                FlightListDG.Items.Add(String.Format("{0, -8} {1,-12} {2,8}", fList[num].Number, fList[num].City, fList[num].Depature_time));
            }
            else
            {
                //FlightListDG.Items[num] = String.Format("{0, -8} {1,-12} {2,8}", fList[num].Number, fList[num].City, fList[num].Depature_time);
            }//
        }*/

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ChangeFlightListData(flightNum);

            if(FlightListDG.SelectedIndex == -1)
            {
                flightAdd = true;
            }

            if (flightAdd)
            {
                fList.Add(new Flight(numFlightTextBox.Text, cityFlightTextBox.Text, TimeSpan.Parse(timeFlightTextBox.Text), Convert.ToInt32(freeSeatsTextBox.Text)));
                sqlCommandCache += $"INSERT INTO `rozklad` (`id`, `number`, `city`, `depature_time`, `free_seats`) VALUES (" +
                    $"{fList.Count},'{numFlightTextBox.Text}', '{cityFlightTextBox.Text}', '{TimeSpan.Parse(timeFlightTextBox.Text)}', {Convert.ToInt32(freeSeatsTextBox.Text)});";
                

            }
            else
            {
                fList[FlightListDG.SelectedIndex].City = cityFlightTextBox.Text;
                fList[FlightListDG.SelectedIndex].Free_seats = Convert.ToInt32(freeSeatsTextBox.Text);
                fList[FlightListDG.SelectedIndex].Number = numFlightTextBox.Text;
                fList[FlightListDG.SelectedIndex].Depature_time = TimeSpan.Parse(timeFlightTextBox.Text);
                sqlCommandCache += $"UPDATE `rozklad` SET `number` = '{numFlightTextBox.Text}', `city` = '{cityFlightTextBox.Text}', `depature_time` = '" +
            $"{TimeSpan.Parse(timeFlightTextBox.Text)}', `free_seats` = {Convert.ToInt32(freeSeatsTextBox.Text)} WHERE `rozklad`.`id` = {FlightListDG.SelectedIndex};";

            }

            Console.WriteLine(sqlCommandCache);

           

            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand command = new MySqlCommand();
            command.Connection = conn;
            command.CommandText = sqlCommandCache;
            command.Connection.Open();
            command.ExecuteNonQuery();

            FlightListDG.Items.Refresh();

        }

        private void MenuItem6_Click(object sender, RoutedEventArgs e)
        {
            flightNum = FlightListDG.Items.Count;
            flightAdd = true;
        }

        private void MenuItem3_Click(object sender, RoutedEventArgs e)
            {
               if(selectXList.Items.Count>0){
                    groupBox2.Visibility= Visibility.Visible;
                    groupBox3.Visibility = Visibility.Hidden;
                }
                else
                {
                    MessageBox.Show("Недостатньо даних!" + char.ConvertFromUtf32(13) + "Спочатку потрібно виконати команду" + char.ConvertFromUtf32(13) +
                        "Пошук - За містом призначення", "Увага", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }

        private void MenuItem4_Click(object sender, RoutedEventArgs e)
        {
           if((groupBox1.Visibility == Visibility.Visible) || (groupBox2.Visibility == Visibility.Visible))
            {
                Button_Click(sender, e);
            }

            if(groupBox3.Visibility == Visibility.Visible)
            {
                //запис зміненого списку рейсів у ексель 
                //запишіть в умову іф код запису у файл списку із даними про рейси
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (FlightListDG.Items.Count > 0) {
              
                if(cityList.SelectedIndex != -1)
                {
                    string selectedCity = cityList.Items[cityList.SelectedIndex].ToString();
                    selectedCityList = SelectX(selectedCity);
                    selectXList.Items.Clear();
                    foreach (Flight it in selectedCityList)
                        selectXList.Items.Add(Convert.ToString(it.Depature_time) + " " + Convert.ToString(it.Free_seats) + " місць") ;

                    selectXYList.Items.Clear();
                    xP = selectedCity;
                }
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {

            if(FlightListDG.Items.Count > 0)
            {
                if(sTime.Text != " " && cityList.Items.Count > 0)
                {
                    TimeSpan depat;
                    if(TimeSpan.TryParse(sTime.Text, out depat))
                    {
                        selectedCityTimeList = SelectXY(depat);
                        selectXYList.Items.Clear();
                        foreach (Flight it in selectedCityTimeList)
                            selectXYList.Items.Add(it.City + "\t" + it.Depature_time + " " + it.Free_seats + " місць");
                        yP = depat;
                    }
                }
            }
        }

        private void WriteData(List <Flight> selXList, List <Flight> selXYList) 
        {
            try
            {
                Console.WriteLine(filePath + "\\Шаблон_Пошуку_рейсів.dot");
                wordApp = new Microsoft.Office.Interop.Word.Application();
                wordDoc = wordApp.Documents.Add(filePath + "\\Шаблон_Пошуку_рейсів.dot");
                string selectedCity = cityList.SelectedItem.ToString();//?
                ReplaceText(selectedCity, "[X]");
                ReplaceText(selXList, 1);

                ReplaceText(sTime.Text, "[Y]");
                ReplaceText(selXYList, 2);
                wordApp.Visible = false;
                wordDoc.Save();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + char.ConvertFromUtf32(13) +
                    "Помістіть файл Шаблон_Пошуку_рейсів.dot" + char.ConvertFromUtf32(13) + "у каталог із exe-файлом програми і повторіть зображення", "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                wordApp.Quit();
            }

            if(wordDoc != null)
            {
                wordDoc.Close();
            }
            if(wordApp != null)
            {
                wordApp.Quit();
            }
        }

        private void ReplaceText(string textToReplace, string replacedText)
        {
            Object missingg = Type.Missing;
            Microsoft.Office.Interop.Word.Range selText;
            selText = wordDoc.Range(wordDoc.Content.Start, wordDoc.Content.End);
            Microsoft.Office.Interop.Word.Find find = wordApp.Selection.Find;
            find.Text = replacedText;
            find.Replacement.Text = textToReplace;
            Object wrap = Microsoft.Office.Interop.Word.WdFindWrap.wdFindContinue;
            Object replace = Microsoft.Office.Interop.Word.WdReplace.wdReplaceAll;

            find.Execute(FindText: Type.Missing, MatchCase: false, MatchWholeWord: false, MatchWildcards: false, MatchSoundsLike: missingg,
                    MatchAllWordForms: false, Forward: true, Wrap: wrap, Format: false, ReplaceWith: missingg, Replace: replace);

        }

        private void ReplaceText(List <Flight> selectedList, int numTable)
        {
            for(int i = 0; i < selectedList.Count; i++)
            {
                if(selectedList[i].Number != null)
                {
                    wordDoc.Tables[numTable].Rows.Add();
                    wordDoc.Tables[numTable].Cell(2 + i, 1).Range.Text = selectedList[i].Number;
                    wordDoc.Tables[numTable].Cell(2 + i, 2).Range.Text = Convert.ToString(selectedList[i].Depature_time);
                    if(numTable == 2)
                    {
                        wordDoc.Tables[numTable].Cell(2 + i, 3).Range.Text = Convert.ToString(selectedList[i].Free_seats);
                    }
                }
            }
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            WriteData(selectedCityList, selectedCityTimeList);
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FlightListDG.SelectedItem = null;
        }
    }



}
