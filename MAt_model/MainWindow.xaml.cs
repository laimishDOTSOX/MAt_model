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

namespace MAt_model
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Size_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Check_input(size_c.Text)) size_c.Text = ""; //Если метод, проверяющий буква вводится или нет, возвращает false строка становится пустой 
            if (!Check_input(size_r.Text)) size_r.Text = "";
            if (size_c.Text != "" & size_r.Text != "")
            {
                Update_vectors(Convert.ToInt32(size_c.Text), Convert.ToInt32(size_r.Text));//создаем в сетках для векторов нужное кол-во колонок и текстовых блоков
                Update_array(Convert.ToInt32(size_c.Text), Convert.ToInt32(size_r.Text));//создаем матрицу
            }
           
        }

        private void ArrayBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //из-за того что кол-во ячеек в матрице не статично, при изменении текста в любой из них пробегаем по всем ячейкам
            int count_row = Array.RowDefinitions.Count;
            int count_col = Array.ColumnDefinitions.Count;
            int none_content_box = 0;//кол-во пустых ячеек
            for (int i = 0; i < count_row; i++)
            {
                for (int c = 0; c < count_col; c++)
                {
                    if (FindName($"ArrayBox{i}_{c}") is TextBox box)
                    {
                        if (!Check_input(box.Text))
                        {
                            box.Text = "";
                            none_content_box++;
                        }
                    }
                }
            }
            if (none_content_box == 1) Decision();//если все возможные ячейки заполнены ( == 1, потомучно левая верхняя ячейка будет всегда пустая)
           
        }

        private void Decision()
        {
            //кол-во строк и солбцов берем на 1 меньше, ибо унас есть "лишние" ячейки по которым не надо проходиться
            int count_col = Array.ColumnDefinitions.Count - 1;
            int count_row = Array.RowDefinitions.Count - 1;
            //создаем массивы векторов, чтобы отслеживать сколько осталось поставить продукции
            int[] col_demand = new int[count_col];
            int[] row_power= new int[count_row];
            int dem = 0;//будет хранить значение поставки (хз как объяснить крч)
            //кол-во строк, в которых закончилась продукция, и столбцов, в которым больше не нужна продукция
            int count_zero_dem = 0;
            int count_zero_pow = 0;
            //заполняем массивы векторов 
            for (int i = 1; i <= count_col; i++)
            {
                TextBox box = FindName($"ArrayBox0_{i}") as TextBox;
                col_demand[i - 1] = Convert.ToInt32(box.Text);
            }
            for (int i = 1; i <= count_row; i++)
            {
                TextBox box = FindName($"ArrayBox{i}_0") as TextBox;
                row_power[i - 1] = Convert.ToInt32(box.Text);
            }
            //проходимся по всем столбцам пока не распределим все поставки
            while (true)
            {
                for (int i = 1; i <= count_col; i++)
                {
                    if (col_demand[i - 1] == 0) count_zero_dem++;
                    else
                    {
                        int cell_row = Convert.ToInt32(Min_expense(i).Substring(8, 1));//находим строчку с минимальным числом в столбце (из полученного имени вырезаем символы
                                                                                       //с 8 (по индексу) длинной в 1 символ
                        dem = Counting(Min_expense(i), col_demand[i - 1], row_power[cell_row-1]);// получаем число поставленных товаров
                        //вычитаем из массивов векторов это число
                        col_demand[i - 1] = col_demand[i - 1] - dem;
                        row_power[cell_row-1] = row_power[cell_row-1] - dem;
                        if (row_power[cell_row - 1] == 0) count_zero_pow++;
                    }
                }
                if (count_zero_dem >= count_col & count_zero_pow >= count_row) break;//если количество "закончившихся" ячеек больше или равно количеству столбцов и строк, то THE END
            }
            //пока считали в предыдущем (если оно пишется не через "ы" то я не знаю) блоке, отключали у блоков проверку на числа
            //ибо приписывали к значению "|", тк что теперь возвращаем им эту проверку 
            for (int i = 1; i <= count_row; i++)
            {
                for (int c = 1; c <= count_col; c++)
                {
                    if (FindName($"ArrayBox{i}_{c}") is TextBox box) box.TextChanged += ArrayBox_TextChanged;
                }
            }
            General_expenses();//считаем общие затраты 
        }

        private void General_expenses()
        {
            int count_col = Array.ColumnDefinitions.Count - 1;
            int count_row = Array.RowDefinitions.Count - 1;
            int sum = 0;
            string f = "F = ";

            for (int r = 1; r <= count_row; r++)
            {
                for (int c = 1; c <= count_col; c++)
                {
                    TextBox box = FindName($"ArrayBox{r}_{c}") as TextBox;
                    if (box.Text.Contains("|"))
                    {
                        f += $"{box.Text.Substring(0, 1)}*{box.Text.Substring(4, 1)} + ";
                        sum += Convert.ToInt32(box.Text.Substring(0, 1)) * Convert.ToInt32(box.Text.Substring(4, 1));
                    }
                }
            }
            f = f.Remove(f.Length - 2) + $"= {sum}";
            ATVET.Text = f;
        }

        private int Counting(string cell,int ost_dem, int ost_pow)
        {
            TextBox box = FindName(cell) as TextBox;
            if (ost_pow > ost_dem)
            {
                box.TextChanged -= ArrayBox_TextChanged;
                box.Text = box.Text + " | " + ost_dem;
                return ost_dem;
            }
            else
            {
                box.TextChanged -= ArrayBox_TextChanged;
                box.Text = box.Text + " | " + ost_pow;
                return ost_pow;
            }
        }

        private string Min_expense(int col)
        {
            int count_row = Array.RowDefinitions.Count;
            List<expense> exp = new List<expense>();//создаем лист, числа в котором потом буудем сравнивать
            for (int r = 1; r < count_row; r++)
            {
                TextBox box = FindName($"ArrayBox{r}_{col}") as TextBox;//находим блок по имени
                if (box.Text.Contains("|")) ;//если он содержит "|" - ничего не делаем 
                else exp.Add(new expense() { name = box.Name, exp = Convert.ToInt32(box.Text) }); //если знак есть - "заносим" число в этом блоке и имя блока в список

            }
            int e = 0;//тут и будет самое маленькое число
            //эти два условия никогда не будут работать ну и ладно 
            if (exp.Count == 0) return exp[0].name;
            if (exp.Count == 1) return exp[0].name;
            //находим самое маленькое число
            for (int i = 0; i < exp.Count-1; i++)
            {
                if (exp[i].exp <= exp[i + 1].exp) e = i;
                else if (exp[i + 1].exp <= exp[i].exp) e = i + 1;
            }
            
            return exp[e].name;//возвращаем имя ячейки с самым маленьким числом
        }

        class expense
        {
            public string name{ get; set; }
            public int exp { get; set; }
        }

        private void Update_array(int a,int b)
        {
            //получаем кол-во столбцов и строк в матрице чтобы удалить их
            int count_col = Array.ColumnDefinitions.Count;
            int count_row = Array.RowDefinitions.Count;
            for (int i = 0; i < count_col; i++)Array.ColumnDefinitions.RemoveAt(0);
            for (int i = 0; i < count_row; i++) Array.RowDefinitions.RemoveAt(0);
            //сохдаем и добавляем в сетку нужжное кол-во строк
            for (int i = 0; i <= b; i++)
            {
                var Row = new RowDefinition();
                Array.RowDefinitions.Add(Row);
            }
            //сохдаем и добавляем в сетку нужжное кол-во столбцов
            for (int i = 0; i <= a; i++)
            {
                var Col = new ColumnDefinition();
                Array.ColumnDefinitions.Add(Col);
            } 
            //проходимся по каждомй столбцу в каждой строке
            for (int i = 0; i <= b; i++)
            {
                for (int e = 0; e <= a; e++)//тут все как в блоках в ввекторах, только контур другой и метод при изменениии текста
                {
                    var text_box = new TextBox();
                    text_box.Name = $"ArrayBox{i}_{e}";
                    try
                    {
                        this.RegisterName(text_box.Name, text_box);
                    }
                    catch
                    {
                        this.UnregisterName(text_box.Name);
                        this.RegisterName(text_box.Name, text_box);
                    }
                    text_box.TextChanged += ArrayBox_TextChanged;
                    text_box.Style = (Style)size_c.FindResource("TextBoxStyle1");
                    text_box.Foreground = new SolidColorBrush(Colors.White);
                    text_box.Background = new SolidColorBrush(Colors.Black);
                    text_box.HorizontalContentAlignment = HorizontalAlignment.Center;
                    text_box.VerticalContentAlignment = VerticalAlignment.Center;
                    text_box.BorderBrush = Brushes.White;
                    text_box.BorderThickness = new Thickness(0, 0, 2, 2);
                    text_box.FontSize = 22;
                    //матрица делается "на 1 больше" чем пишет пользователь, в этих "лишних" ячейках будут писаться значения из векторов и в них самому писать нельзя
                    if (i == 0)text_box.IsReadOnly = true;
                    else if(e == 0) text_box.IsReadOnly = true;
                    Array.Children.Add(text_box);
                    Grid.SetRow(text_box, i);
                    Grid.SetColumn(text_box, e);
                }
                
            }
        }

        private void Update_vectors(int a, int b)//получает числа введенные в size_r и size_c
        {
            int count_power = power_vector.ColumnDefinitions.Count; //получаем кол-во колонок 
            int count_demand = demand_vector.ColumnDefinitions.Count;
            for (int i = 0; i < count_power; i++)power_vector.ColumnDefinitions.RemoveAt(0);//удаляем по очереди все колонки
            for (int i = 0; i < count_demand; i++) demand_vector.ColumnDefinitions.RemoveAt(0);

            for (int i = 0; i < b; i++)//создаем кол-во колонок и текстовых блоков равное чилу введенному в size_r
            {
                var text_box = new TextBox();//создаем экземпляр объекта TextBox
                var Col = new ColumnDefinition();//создаем экземпляр объекта ColumnDefinition
                power_vector.ColumnDefinitions.Add(Col);
                text_box.Foreground = new SolidColorBrush(Colors.White);//цвет текста
                text_box.Background = new SolidColorBrush(Colors.Black);//цвет фона
                text_box.TextAlignment = TextAlignment.Center;//выравнивание по центру 
                text_box.Style = (Style)size_c.FindResource("TextBoxStyle1");//применяем стиль блока (там просто цвет выделения блока на белый поменял)
                text_box.Name = $"Power_{i}";//присваеваем блоку имя, чтобы потом по имени получать из него значение
                text_box.FontSize = 16;
                try
                {
                    this.RegisterName(text_box.Name, text_box);//пытаемся зарегистрировать имя
                }
                catch//если такое имя есть, удаляем его и еще раз регистрируем
                {
                    this.UnregisterName(text_box.Name);
                    this.RegisterName(text_box.Name, text_box);
                }
                text_box.TextChanged += Power_TextChanged;//добавляем метод еоторый будет вызываться при изменениии значения в блоке 
                if (i != 0)
                {
                    text_box.BorderBrush = new SolidColorBrush(Colors.White);//цвет контура
                    text_box.BorderThickness = new Thickness(2, 0, 0, 0);//ширина контура (справо,верх,слево,низ)
                }
                else text_box.BorderThickness = new Thickness(0);
                power_vector.Children.Add(text_box);//"засовываем" получившийся блок в сетку power_vector
                Grid.SetColumn(text_box, i);//выбираем колонку для элемента
            }
            //тут тоже самое только для другой 
            for (int i = 0; i < a; i++)//создаем кол-во колонок и текстовых блоков равное чилу введенному в size_с
            {
                var text_box = new TextBox();
                var Col = new ColumnDefinition();
                demand_vector.ColumnDefinitions.Add(Col);
                text_box.Foreground = new SolidColorBrush(Colors.White);
                text_box.Background = new SolidColorBrush(Colors.Black);
                text_box.TextAlignment = TextAlignment.Center;
                text_box.Style = (Style)size_c.FindResource("TextBoxStyle1");
                text_box.Name = $"Demand_{i}";
                text_box.FontSize = 16;
                try
                {
                    this.RegisterName(text_box.Name, text_box);
                }
                catch
                {
                    this.UnregisterName(text_box.Name);
                    this.RegisterName(text_box.Name, text_box);
                }
                text_box.TextChanged += Demand_TextChanged;
                if (i != 0)
                {
                    text_box.BorderBrush = new SolidColorBrush(Colors.White);
                    text_box.BorderThickness = new Thickness(2, 0, 0, 0);
                }
                else text_box.BorderThickness = new Thickness(0);
                demand_vector.Children.Add(text_box);
                Grid.SetColumn(text_box, i);
            }
        }

        private void Power_TextChanged(object sender, TextChangedEventArgs e)
        {
            int count = power_vector.ColumnDefinitions.Count;
            for (int i = 0; i < count; i++)
            {
                if (FindName($"Power_{i}") is TextBox box)
                {
                    if (!Check_input(box.Text)) box.Text = "";
                      else
                    {
                        if (Sum_vectors())
                        {
                            ATVET.Foreground = Brushes.White;
                            ATVET.Text = "";
                        }
                        else
                        {
                            ATVET.Foreground = Brushes.Red;
                            ATVET.Text = "Суммы векторов не равны!!!";
                        }
                    }
                    if (FindName($"ArrayBox{i + 1}_0") is TextBox value_vector)value_vector.Text = box.Text;
                }
            }
        }

        private void Demand_TextChanged(object sender, TextChangedEventArgs e)
        {
            int count = demand_vector.ColumnDefinitions.Count;
            for (int i = 0; i < count; i++)
            {
                if (FindName($"Demand_{i}") is TextBox box)
                {
                    if (!Check_input(box.Text)) box.Text = "";
                    else
                    {
                        if (Sum_vectors())
                        {
                            ATVET.Foreground = Brushes.White;
                            ATVET.Text = "";
                        }
                        else
                        {
                            ATVET.Foreground = Brushes.Red;
                            ATVET.Text = "Суммы векторов не равны!!!";
                        }
                    }
                    if (FindName($"ArrayBox0_{i + 1}") is TextBox value_vector) value_vector.Text = box.Text;
                }
            }
        }

        private bool Sum_vectors()
        {
            int sum_power = 0;
            int sum_demand= 0;
            for (int i = 0; i < power_vector.ColumnDefinitions.Count; i++)
            {
                if (FindName($"Power_{i}") is TextBox box)
                {
                    if (box.Text == "") return true;
                    sum_power += Convert.ToInt32(box.Text);
                }
            }
            for (int i = 0; i < demand_vector.ColumnDefinitions.Count; i++)
            {
                if (FindName($"Demand_{i}") is TextBox box)
                {
                    if (box.Text == "") return true;
                    sum_demand += Convert.ToInt32(box.Text);
                }
            }
            if (sum_demand != sum_power) return false;
            else return true;
        }

        private bool Check_input(string a)
        {
            try
            {
                int i = Convert.ToInt32(a);
                return true;
            }
            catch (System.FormatException)
            {
                return false;
            }
            
        }
    }
}
