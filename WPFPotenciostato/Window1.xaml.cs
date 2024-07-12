using System;
using System.Collections.Generic;
using System.IO.Ports;
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

namespace WPFPotenciostato
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private SerialPort _serialPort;
        private String selectedSerialPort;
        private int baudRate;
        public Window1()
        {
            InitializeComponent();
        }
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string inData = _serialPort.ReadLine();
            Dispatcher.Invoke(() => { TextBoxOutput.AppendText(inData + "\n"); });
        }
        private void InitializeSerialPort()
        {
            _serialPort = new SerialPort(selectedSerialPort, baudRate);
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            _serialPort.Open();
        }
        private void Connect_Comms(object sender, RoutedEventArgs e)
        {
            selectedSerialPort = COMselect.Text;
            baudRate = Convert.ToInt32(Baudselect.Text);
            InitializeSerialPort();
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        private void DisconnectSerialPort(object sender, RoutedEventArgs e)
        { 
            _serialPort.Close();
        }
    }
}
