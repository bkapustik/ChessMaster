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
using System.IO.Ports;

namespace ControllerApp
{
    /// <summary>
    /// Interaction logic for PortSelectDialog.xaml
    /// </summary>
    public partial class PortSelectDialog : Window
    {
        public PortSelectDialog()
        {
            InitializeComponent();
        }

        public string QueryPort()
        {
            comboBoxPorts.Items.Clear();
            foreach (var port in SerialPort.GetPortNames())
                comboBoxPorts.Items.Add(port);
            
            comboBoxPorts.Items.Add("DUMMY");
            
            if (comboBoxPorts.Items.Count == 0)
            {
                MessageBox.Show("There are no available serial ports!", "No Ports", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            comboBoxPorts.SelectedIndex = 0;

            if (ShowDialog() != true) return null;
            return comboBoxPorts.SelectedItem.ToString();
        }

        private void buttonSelect_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
