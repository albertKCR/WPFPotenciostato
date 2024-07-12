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
using System.IO.Ports;
using System.Threading;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Definitions.Charts;
using LiveCharts.Defaults;

namespace WPFPotenciostato
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Serial Port Variables
        private SerialPort _serialPort;
        private String selectedSerialPort;
        private int baudRate;
        #endregion

        #region Config Panel Items
        private StackPanel LSV_innerStackPanelTimeStep;
        private StackPanel LSV_innerStackPanelVoltageStep;
        private StackPanel LSV_innerStackPanelInitialVoltage;
        private StackPanel LSV_innerStackPanelFinalVoltage;
        private TextBox LSV_TimeStep;
        private TextBox LSV_VoltageStep;
        private TextBox LSV_TimeStepText;
        private TextBox LSV_VoltageStepText;
        private TextBox LSV_InitialVoltageText;
        private TextBox LSV_InitialVoltage;
        private TextBox LSV_FinalVoltageText;
        private TextBox LSV_FinalVoltage;
        private Button LSV_SendParameters;
        #endregion

        bool IsInMeasure = false;
        String VoltagePoints;
        float[] VoltagePointsFloat;
        public SeriesCollection CurrentSeries { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            
            CurrentSeries = new SeriesCollection // Initialize a empty chart series
            {
                new LineSeries
                {
                    Title = "Current",
                    Values = new ChartValues<double> {}
                }
            };
            DataContext = this;
        }

        private void OpenSecondWindow_Click(object sender, RoutedEventArgs e)
        {
            Window1 secondWindow = new Window1();
            secondWindow.Show();
        }

        #region Serial Port
        private void InitializeSerialPort()
        {
            try
            {
                _serialPort = new SerialPort(selectedSerialPort, baudRate);
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(CurrentPointsPlotter);
                _serialPort.Open();
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
            }
            catch
            {
                MessageBox.Show("Nonexistent COM or incomplete form.", "Error");
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string inData = _serialPort.ReadLine();
            //Dispatcher.Invoke(() => {TextBoxOutput.AppendText(inData + "\n"); });
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
        private void Connect_Comms(object sender, RoutedEventArgs e)
        {
            selectedSerialPort = COMselect.Text;
            baudRate = Convert.ToInt32(Baudselect.Text);
            InitializeSerialPort();
        }
        private void DisconnectSerialPort(object sender, RoutedEventArgs e)
        {
            _serialPort.Close();
        }
        #endregion

        #region LSV Config Panel
        private void LSV_SendMeasureParameters(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(LSV_TimeStep.Text) && !string.IsNullOrEmpty(LSV_VoltageStep.Text)
                && !string.IsNullOrEmpty(LSV_InitialVoltage.Text) && !string.IsNullOrEmpty(LSV_FinalVoltage.Text))
            {
                MessageBox.Show("Starting measure.");
                VoltagePointsArrayCalculator(float.Parse(LSV_TimeStep.Text), float.Parse(LSV_InitialVoltage.Text), float.Parse(LSV_FinalVoltage.Text));
                IsInMeasure = true;
            }
            else MessageBox.Show("Fill all the parameters to make the measure.", "Error");
        }
        #region LSV Config Panel Design
        private void LSV_CreateConfigPanelItems()
        {
            LSV_innerStackPanelTimeStep = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            LSV_innerStackPanelVoltageStep = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            LSV_innerStackPanelInitialVoltage = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            LSV_innerStackPanelFinalVoltage = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };

            LSV_TimeStepText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Time Step",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            LSV_TimeStep = new TextBox
            {
                Width = 50,
                Height = 20
            };
            LSV_VoltageStepText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Voltage Step",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            LSV_VoltageStep = new TextBox
            {
                Width = 50,
                Height = 20
            };
            LSV_InitialVoltageText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Initial Voltage",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            LSV_InitialVoltage = new TextBox
            {
                Width = 50,
                Height = 20
            };
            LSV_FinalVoltageText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Final Voltage",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            LSV_FinalVoltage = new TextBox
            {
                Width = 50,
                Height = 20
            };
            LSV_SendParameters = new Button
            {
                Width = 80,
                Height = 20,
                Content = "Start Measure"
            };
            LSV_SendParameters.Click += LSV_SendMeasureParameters;
        }
        private void LSV_InitializeConfigPanelItems()
        {
            configPanel.Children.Add(LSV_innerStackPanelTimeStep);
            LSV_innerStackPanelTimeStep.Children.Add(LSV_TimeStepText);
            LSV_innerStackPanelTimeStep.Children.Add(LSV_TimeStep);
            configPanel.Children.Add(LSV_innerStackPanelVoltageStep);
            LSV_innerStackPanelVoltageStep.Children.Add(LSV_VoltageStepText);
            LSV_innerStackPanelVoltageStep.Children.Add(LSV_VoltageStep);
            configPanel.Children.Add(LSV_innerStackPanelInitialVoltage);
            LSV_innerStackPanelInitialVoltage.Children.Add(LSV_InitialVoltageText);
            LSV_innerStackPanelInitialVoltage.Children.Add(LSV_InitialVoltage);
            configPanel.Children.Add(LSV_innerStackPanelFinalVoltage);
            LSV_innerStackPanelFinalVoltage.Children.Add(LSV_FinalVoltageText);
            LSV_innerStackPanelFinalVoltage.Children.Add(LSV_FinalVoltage);
            Rectangle spacer = new Rectangle { Width = 40 };
            LSV_innerStackPanelFinalVoltage.Children.Add(spacer);
            LSV_innerStackPanelFinalVoltage.Children.Add(LSV_SendParameters);
        }
        #endregion
        #endregion

        private void ConfigSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConfigSelect.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedContent = selectedItem.Content.ToString();

                switch (selectedContent)
                {
                    case "Linear Sweep Voltammetry":
                        CurrentSeries.Clear();
                        configPanel.Children.Clear();
                        LSV_CreateConfigPanelItems();
                        LSV_InitializeConfigPanelItems();
                        break;
                    case "Cyclic Voltammetry":
                        CurrentSeries.Clear();
                        configPanel.Children.Clear();
                        MessageBox.Show("Cyclic Voltammetry");
                        break;
                    case "Differential Pulse Voltammetry":
                        CurrentSeries.Clear();
                        configPanel.Children.Clear();
                        MessageBox.Show("Differential Pulse Voltammetry");
                        break;
                    case "Normal Pulse Voltammetry":
                        CurrentSeries.Clear();
                        configPanel.Children.Clear();
                        MessageBox.Show("Normal Pulse Voltammetry");
                        break;
                    default:

                        break;

                }
            }
        }
        private void VoltagePointsArrayCalculator(float VoltageStep, float InitialVoltage, float FinalVoltage)
        {
            int PointsNumber = (int)((FinalVoltage - InitialVoltage) / (VoltageStep*0.001));
            VoltagePoints = InitialVoltage.ToString();
            double Accumulator = InitialVoltage;

            for (int i = 1; i < PointsNumber; i++)
            {
                Accumulator = Accumulator + VoltageStep*0.001;
                if (Accumulator > FinalVoltage) VoltagePoints = VoltagePoints + "," + FinalVoltage.ToString();
                VoltagePoints = VoltagePoints + "," + Accumulator.ToString();
            }
            VoltagePoints = VoltagePoints + "," + FinalVoltage.ToString();
            _serialPort.Write(VoltagePoints);

            VoltagePointsFloat = ConvertStringToFloatArray(VoltagePoints);
        }
        private void CurrentPointsPlotter(object sender, SerialDataReceivedEventArgs e)
        {
            if (IsInMeasure)
            {
                Thread.Sleep(1000);
                string input = _serialPort.ReadExisting();
                _serialPort.DiscardInBuffer();
                float[] MeasuredCurrent = ConvertStringToFloatArray(input);

                Dispatcher.Invoke(() =>
                {
                    CurrentSeries.Clear();
                    var ivPoints = new ChartValues<ObservablePoint>();

                    for (int i = 0; i < VoltagePointsFloat.Length; i++)
                    {
                        ivPoints.Add(new ObservablePoint(VoltagePointsFloat[i], MeasuredCurrent[i]));
                    }

                    var ivSeries = new LineSeries
                    {
                        Title = "IV Curve",
                        Values = ivPoints,
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 5
                    };
                    
                    CurrentSeries.Add(ivSeries);
                });
                Thread.Sleep(1200);
            }
        }
        static float[] ConvertStringToFloatArray(string input)
        {
            string[] stringArray = input.Split(',');

            float[] floatArray = new float[stringArray.Length];
            for (int i = 0; i < stringArray.Length; i++)
            {
                float.TryParse(stringArray[i], out floatArray[i]);
            }

            return floatArray;
        }
    }
}
