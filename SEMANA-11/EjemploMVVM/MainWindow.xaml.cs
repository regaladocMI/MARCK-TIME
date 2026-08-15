using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EjemploMVVM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string cn = "Server=localhost;Database=Northwind;User Id=sa;Password=123456;";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnCargar_click(object sender, RoutedEventArgs e)
        {
            string query = "SELECT ProductID, ProductName, UnitPrice, Discontinued FROM Products";
            using (SqlConnection conex = new SqlConnection(cn))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conex);
                DataTable dtProductos = new DataTable();
                da.Fill(dtProductos);

                dgProductos.ItemsSource = dtProductos.DefaultView;
            }
        }
    }
}