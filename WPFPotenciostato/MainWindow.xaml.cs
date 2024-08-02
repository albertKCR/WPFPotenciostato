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
using System.IO;
using LiveCharts.Wpf.Charts.Base;
using System.Timers;

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
        private StackPanel LSV_innerStackPanelLogSend;
        private TextBox LSV_TimeStep;
        private TextBox LSV_VoltageStep;
        private TextBox LSV_TimeStepText;
        private TextBox LSV_VoltageStepText;
        private TextBox LSV_InitialVoltageText;
        private TextBox LSV_InitialVoltage;
        private TextBox LSV_FinalVoltageText;
        private TextBox LSV_FinalVoltage;
        private Button LSV_SendParameters;

        private StackPanel CV_VerticalPanel1;
        private StackPanel CV_VerticalPanel2;
        private StackPanel CV_innerStackPanelTimeStep;
        private StackPanel CV_innerStackPanelVoltageStep;
        private StackPanel CV_innerStackPanelInitialVoltage;
        private StackPanel CV_innerStackPanelFinalVoltage;
        private StackPanel CV_innerStackPanelPeakVoltage;
        private StackPanel CV_innerStackPanelCycles;
        private TextBox CV_TimeStep;
        private TextBox CV_VoltageStep;
        private TextBox CV_InitialVoltage;
        private TextBox CV_FinalVoltage;
        private TextBox CV_PeakVoltage;
        private TextBox CV_Cycles;
        private TextBox CV_TimeStepText;
        private TextBox CV_VoltageStepText;
        private TextBox CV_InitialVoltageText;
        private TextBox CV_FinalVoltageText;
        private TextBox CV_PeakVoltageText;
        private TextBox CV_CyclesText;
        private Button CV_SendParameters;

        private StackPanel DPV_VerticalPanel1;
        private StackPanel DPV_VerticalPanel2;
        private StackPanel DPV_innerStackPanelPulseTime;
        private StackPanel DPV_innerStackPanelVoltageStep;
        private StackPanel DPV_innerStackPanelInitialVoltage;
        private StackPanel DPV_innerStackPanelFinalVoltage;
        private StackPanel DPV_innerStackPanelPulseVoltage;
        private StackPanel DPV_innerStackPanelLowTime;
        private TextBox DPV_PulseTime;
        private TextBox DPV_VoltageStep;
        private TextBox DPV_InitialVoltage;
        private TextBox DPV_FinalVoltage;
        private TextBox DPV_PulseVoltage;
        private TextBox DPV_LowTime;
        private TextBox DPV_PulseTimeText;
        private TextBox DPV_VoltageStepText;
        private TextBox DPV_InitialVoltageText;
        private TextBox DPV_FinalVoltageText;
        private TextBox DPV_PulseVoltageText;
        private TextBox DPV_LowTimeText;
        private Button DPV_SendParameters;

        private StackPanel NPV_VerticalPanel1;
        private StackPanel NPV_VerticalPanel2;
        private StackPanel NPV_innerStackPanelPulseTime;
        private StackPanel NPV_innerStackPanelVoltageStep;
        private StackPanel NPV_innerStackPanelInitialVoltage;
        private StackPanel NPV_innerStackPanelFinalVoltage;
        private StackPanel NPV_innerStackPanelLowTime;
        private TextBox NPV_PulseTime;
        private TextBox NPV_VoltageStep;
        private TextBox NPV_InitialVoltage;
        private TextBox NPV_FinalVoltage;
        private TextBox NPV_LowTime;
        private TextBox NPV_PulseTimeText;
        private TextBox NPV_VoltageStepText;
        private TextBox NPV_InitialVoltageText;
        private TextBox NPV_FinalVoltageText;
        private TextBox NPV_LowTimeText;
        private Button NPV_SendParameters;

        private CheckBox CurrentInLog;
        bool LogIsChecked;
        #endregion

        bool IsInMeasure = false;
        String VoltagePoints;
        float[] VoltagePointsFloat;
        public SeriesCollection CurrentSeries { get; set; }
        private LineSeries ivSeries;

        int VoltageArrayIndexOffset = 0;
        bool NotFirstData = false;
        float[] MeasuredCurrent;
        public MainWindow()
        {
            InitializeComponent();

            string[] ports = SerialPort.GetPortNames();

            for (int i = 0; i < ports.Length; i++)
            {
                COMselect.Items.Add(ports[i]);
            }
            CurrentSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Current",
                    Values = new ChartValues<double> {}
                }
            };
            DataContext = this;
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

        #region Linear Sweep Voltammetry Config
        private void LSV_SendMeasureParameters(Object sender, RoutedEventArgs e)
        {
            CurrentSeries.Clear();
            if (ivSeries != null)
            {
                ivSeries.Values.Clear();
            }
            if (!string.IsNullOrEmpty(LSV_TimeStep.Text) && !string.IsNullOrEmpty(LSV_VoltageStep.Text)
                && !string.IsNullOrEmpty(LSV_InitialVoltage.Text) && !string.IsNullOrEmpty(LSV_FinalVoltage.Text))
            {
                MessageBox.Show("Starting measure.");
                LSVVoltagePointsArrayCalculator(float.Parse(LSV_TimeStep.Text), float.Parse(LSV_InitialVoltage.Text)
                    , float.Parse(LSV_FinalVoltage.Text), float.Parse(LSV_VoltageStep.Text));
                IsInMeasure = true;
                _serialPort.Write("0" + "," + LSV_TimeStep.Text + "," + LSV_VoltageStep.Text + "," + LSV_InitialVoltage.Text 
                    + "," + LSV_FinalVoltage.Text);
            }
            else MessageBox.Show("Fill all the parameters to make the measure.", "Error");
        }
        private void LSVVoltagePointsArrayCalculator(float TimeStep, float InitialVoltage, float FinalVoltage, float VoltageStep)
        {
            VoltagePoints = "";
            VoltageStep = VoltageStep * 0.001f;
            int LSVVoltageStepbit = (int)(VoltageStep * 4095) / 5;
            int LSVInitialVoltagebit = (int)(InitialVoltage * 4095) / 5;
            int LSVFinalVoltagebit = (int)(FinalVoltage * 4095) / 5;
            double Accumulator = InitialVoltage;

            for (float i = LSVInitialVoltagebit; i <= LSVFinalVoltagebit; i = i + LSVVoltageStepbit)
            {
                Accumulator = Accumulator + VoltageStep;
                VoltagePoints = VoltagePoints + "," + Accumulator.ToString();
            }
            Console.WriteLine(LSVFinalVoltagebit);
            Console.WriteLine(VoltagePoints);
            //_serialPort.Write(VoltagePoints);

            VoltagePointsFloat = ConvertStringToFloatArray(VoltagePoints);
            VoltagePointsFloat = VoltagePointsFloat.Skip(1).ToArray();
            VoltageArrayIndexOffset = 0;
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
            LSV_innerStackPanelLogSend = new StackPanel
            {
                Orientation = Orientation.Vertical,
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
            CurrentInLog = new CheckBox
            {
                Content = "Current in Log",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 0)
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
            configPanel.Children.Add(LSV_innerStackPanelLogSend);
            Rectangle spacer2 = new Rectangle { Height = 20 };
            LSV_innerStackPanelLogSend.Children.Add(spacer2);
            LSV_innerStackPanelLogSend.Children.Add(CurrentInLog);
            Rectangle spacer3 = new Rectangle { Height = 10 };
            LSV_innerStackPanelLogSend.Children.Add(spacer3);
            LSV_innerStackPanelLogSend.Children.Add(LSV_SendParameters);
            CurrentInLog.Checked += CheckBox_Checked;
            CurrentInLog.Unchecked += CheckBox_Unchecked;
        }
        #endregion
        #endregion

        #region Cyclic Voltammetry Config
        private void CV_SendMeasureParameters(Object sender, RoutedEventArgs e)
        {
            CurrentSeries.Clear();
            if (ivSeries != null)
            {
                ivSeries.Values.Clear();
            }
            if (!string.IsNullOrEmpty(CV_TimeStep.Text) && !string.IsNullOrEmpty(CV_VoltageStep.Text)
                && !string.IsNullOrEmpty(CV_InitialVoltage.Text) && !string.IsNullOrEmpty(CV_FinalVoltage.Text) &&
                !string.IsNullOrEmpty(CV_PeakVoltage.Text) && !string.IsNullOrEmpty(CV_Cycles.Text))
            {
                MessageBox.Show("Starting measure.");
                CVVoltagePointsArrayCalculator(float.Parse(CV_VoltageStep.Text), float.Parse(CV_InitialVoltage.Text),
                    float.Parse(CV_FinalVoltage.Text), float.Parse(CV_Cycles.Text), float.Parse(CV_PeakVoltage.Text),
                    float.Parse(CV_TimeStep.Text));
                IsInMeasure = true;
                _serialPort.Write("1" + "," + CV_TimeStep.Text + "," + CV_VoltageStep.Text + "," + CV_InitialVoltage.Text
                    + "," + CV_FinalVoltage.Text + "," + CV_PeakVoltage.Text + "," + CV_Cycles.Text);
            }
            else MessageBox.Show("Fill all the parameters to make the measure.", "Error");
        }
        private void CVVoltagePointsArrayCalculator(float VoltageStep, float InitialVoltage, float FinalVoltage, 
            float Cycles, float PeakVoltage, float TimeStep)
        {
            VoltageStep = VoltageStep * 0.001f;
            VoltagePoints = "";

            int CVVoltageStepbit = (int)(VoltageStep * 4095) / 5;
            int CVInitialVoltagebit = (int)(InitialVoltage * 4095) / 5;
            int CVFinalVoltagebit = (int)(FinalVoltage * 4095) / 5;
            int CVPeakVoltagebit = (int)(PeakVoltage * 4095) / 5;
            double Accumulator = InitialVoltage;

            for (int cycle = 0; cycle < Cycles; cycle++)
            {
                for (float v = CVInitialVoltagebit; v <= CVPeakVoltagebit; v += CVVoltageStepbit)
                {
                    Accumulator = Accumulator + VoltageStep;
                    VoltagePoints = VoltagePoints + "," + Accumulator.ToString();
                }
                for (float v = CVPeakVoltagebit; v >= CVFinalVoltagebit; v -= CVVoltageStepbit)
                {
                    Accumulator = Accumulator - VoltageStep;
                    VoltagePoints = VoltagePoints + "," + Accumulator.ToString();
                }
            }
            VoltagePointsFloat = ConvertStringToFloatArray(VoltagePoints);
            VoltageArrayIndexOffset = 0;
        }

        #region CV Config Panel Design
        private void CV_CreateConfigPanelItems()
        {
            CV_VerticalPanel1 = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 0, 10)
            };
            CV_VerticalPanel2 = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 0, 10)
            };
            CV_innerStackPanelTimeStep = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            CV_innerStackPanelVoltageStep = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            CV_innerStackPanelInitialVoltage = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            CV_innerStackPanelFinalVoltage = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            CV_innerStackPanelPeakVoltage = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            CV_innerStackPanelCycles = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };

            CV_TimeStepText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Time Step",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            CV_TimeStep = new TextBox
            {
                Width = 50,
                Height = 20
            };
            CV_VoltageStepText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Voltage Step",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            CV_VoltageStep = new TextBox
            {
                Width = 50,
                Height = 20
            };
            CV_InitialVoltageText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Initial Voltage",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            CV_InitialVoltage = new TextBox
            {
                Width = 50,
                Height = 20
            };
            CV_FinalVoltageText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Final Voltage",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            CV_FinalVoltage = new TextBox
            {
                Width = 50,
                Height = 20
            };
            CV_PeakVoltageText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Peak Voltage",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            CV_PeakVoltage = new TextBox
            {
                Width = 50,
                Height = 20
            };
            CV_CyclesText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Cycles",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            CV_Cycles = new TextBox
            {
                Width = 50,
                Height = 20
            };
            CV_SendParameters = new Button
            {
                Width = 80,
                Height = 20,
                Content = "Start Measure"
            };
            CurrentInLog = new CheckBox
            {
                Content = "Plot Current in Log",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 0)
            };
            CV_SendParameters.Click += CV_SendMeasureParameters;
        }
        private void CV_InitializeConfigPanelItems()
        {
            configPanel.Children.Add(CV_VerticalPanel1);
            configPanel.Children.Add(CV_VerticalPanel2);

            CV_VerticalPanel1.Children.Add(CV_innerStackPanelTimeStep);
            CV_innerStackPanelTimeStep.Children.Add(CV_TimeStepText);
            CV_innerStackPanelTimeStep.Children.Add(CV_TimeStep);

            CV_VerticalPanel1.Children.Add(CV_innerStackPanelVoltageStep);
            CV_innerStackPanelVoltageStep.Children.Add(CV_VoltageStepText);
            CV_innerStackPanelVoltageStep.Children.Add(CV_VoltageStep);

            CV_VerticalPanel1.Children.Add(CV_innerStackPanelInitialVoltage);
            CV_innerStackPanelInitialVoltage.Children.Add(CV_InitialVoltageText);
            CV_innerStackPanelInitialVoltage.Children.Add(CV_InitialVoltage);

            CV_VerticalPanel1.Children.Add(CV_innerStackPanelFinalVoltage);
            CV_innerStackPanelFinalVoltage.Children.Add(CV_FinalVoltageText);
            CV_innerStackPanelFinalVoltage.Children.Add(CV_FinalVoltage);

            CV_VerticalPanel2.Children.Add(CV_innerStackPanelPeakVoltage);
            CV_innerStackPanelPeakVoltage.Children.Add(CV_PeakVoltageText);
            CV_innerStackPanelPeakVoltage.Children.Add(CV_PeakVoltage);

            CV_VerticalPanel2.Children.Add(CV_innerStackPanelCycles);
            CV_innerStackPanelCycles.Children.Add(CV_CyclesText);
            CV_innerStackPanelCycles.Children.Add(CV_Cycles);

            Rectangle spacer = new Rectangle { Width = 40 };
            CV_innerStackPanelFinalVoltage.Children.Add(spacer);
            CV_VerticalPanel2.Children.Add(CurrentInLog);
            CV_VerticalPanel2.Children.Add(CV_SendParameters);
            CurrentInLog.Checked += CheckBox_Checked;
            CurrentInLog.Unchecked += CheckBox_Unchecked;
        }
        #endregion
        #endregion

        #region Differential Pulse Voltammetry
        private void DPV_SendMeasureParameters(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(DPV_PulseTime.Text) && !string.IsNullOrEmpty(DPV_VoltageStep.Text)
                && !string.IsNullOrEmpty(DPV_InitialVoltage.Text) && !string.IsNullOrEmpty(DPV_FinalVoltage.Text) &&
                !string.IsNullOrEmpty(DPV_PulseVoltage.Text) && !string.IsNullOrEmpty(DPV_LowTime.Text))
            {
                MessageBox.Show("Starting measure.");
                DPVVoltagePointsArrayCalculator(float.Parse(DPV_VoltageStep.Text), float.Parse(DPV_InitialVoltage.Text),
                    float.Parse(DPV_FinalVoltage.Text), float.Parse(DPV_PulseVoltage.Text), float.Parse(DPV_LowTime.Text),
                    float.Parse(DPV_PulseTime.Text));
                IsInMeasure = true;
                _serialPort.Write("2" + DPV_PulseTime.Text + DPV_VoltageStep.Text + DPV_InitialVoltage.Text
                    + DPV_FinalVoltage.Text + DPV_PulseVoltage.Text + DPV_LowTime.Text);
            }
            else MessageBox.Show("Fill all the parameters to make the measure.", "Error");
        }
        private void DPVVoltagePointsArrayCalculator(float VoltageStep, float InitialVoltage, float FinalVoltage, 
            float PulseVoltage, float LowTime, float PulseTime)
        {
            PulseVoltage = PulseVoltage * 0.001f;
            VoltageStep = VoltageStep * 0.001f;
            VoltagePoints = InitialVoltage.ToString();
            float lastVoltage = InitialVoltage;

            while (lastVoltage < FinalVoltage)
            {
                lastVoltage = lastVoltage + PulseVoltage;
                VoltagePoints = VoltagePoints + "," + lastVoltage.ToString();
                lastVoltage = lastVoltage - VoltageStep;
                VoltagePoints = VoltagePoints + "," + lastVoltage.ToString();
            }
            VoltagePoints = VoltagePoints + "," + FinalVoltage.ToString();
            _serialPort.Write(VoltagePoints);
            VoltagePointsFloat = ConvertStringToFloatArray(VoltagePoints);
        }

        #region DPV Config Panel Design
        private void DPV_CreateConfigPanelItems()
        {
            DPV_VerticalPanel1 = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 0, 10)
            };
            DPV_VerticalPanel2 = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 0, 10)
            };
            DPV_innerStackPanelPulseTime = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            DPV_innerStackPanelVoltageStep = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            DPV_innerStackPanelInitialVoltage = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            DPV_innerStackPanelFinalVoltage = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            DPV_innerStackPanelPulseVoltage = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            DPV_innerStackPanelLowTime = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };

            DPV_PulseTimeText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Pulse Time",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            DPV_PulseTime = new TextBox
            {
                Width = 50,
                Height = 20
            };
            DPV_VoltageStepText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Voltage Step",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            DPV_VoltageStep = new TextBox
            {
                Width = 50,
                Height = 20
            };
            DPV_InitialVoltageText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Initial Voltage",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            DPV_InitialVoltage = new TextBox
            {
                Width = 50,
                Height = 20
            };
            DPV_FinalVoltageText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Final Voltage",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            DPV_FinalVoltage = new TextBox
            {
                Width = 50,
                Height = 20
            };
            DPV_PulseVoltageText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Pulse Voltage",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            DPV_PulseVoltage = new TextBox
            {
                Width = 50,
                Height = 20
            };
            DPV_LowTimeText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Low Time",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            DPV_LowTime = new TextBox
            {
                Width = 50,
                Height = 20
            };
            DPV_SendParameters = new Button
            {
                Width = 80,
                Height = 20,
                Content = "Start Measure"
            };
            CurrentInLog = new CheckBox
            {
                Content = "Plot Current in Log",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 0)
            };
            DPV_SendParameters.Click += DPV_SendMeasureParameters;
        }
        private void DPV_InitializeConfigPanelItems()
        {
            configPanel.Children.Add(DPV_VerticalPanel1);
            configPanel.Children.Add(DPV_VerticalPanel2);

            DPV_VerticalPanel1.Children.Add(DPV_innerStackPanelPulseTime);
            DPV_innerStackPanelPulseTime.Children.Add(DPV_PulseTimeText);
            DPV_innerStackPanelPulseTime.Children.Add(DPV_PulseTime);

            DPV_VerticalPanel1.Children.Add(DPV_innerStackPanelVoltageStep);
            DPV_innerStackPanelVoltageStep.Children.Add(DPV_VoltageStepText);
            DPV_innerStackPanelVoltageStep.Children.Add(DPV_VoltageStep);

            DPV_VerticalPanel1.Children.Add(DPV_innerStackPanelInitialVoltage);
            DPV_innerStackPanelInitialVoltage.Children.Add(DPV_InitialVoltageText);
            DPV_innerStackPanelInitialVoltage.Children.Add(DPV_InitialVoltage);

            DPV_VerticalPanel1.Children.Add(DPV_innerStackPanelFinalVoltage);
            DPV_innerStackPanelFinalVoltage.Children.Add(DPV_FinalVoltageText);
            DPV_innerStackPanelFinalVoltage.Children.Add(DPV_FinalVoltage);

            DPV_VerticalPanel2.Children.Add(DPV_innerStackPanelPulseVoltage);
            DPV_innerStackPanelPulseVoltage.Children.Add(DPV_PulseVoltageText);
            DPV_innerStackPanelPulseVoltage.Children.Add(DPV_PulseVoltage);

            DPV_VerticalPanel2.Children.Add(DPV_innerStackPanelLowTime);
            DPV_innerStackPanelLowTime.Children.Add(DPV_LowTimeText);
            DPV_innerStackPanelLowTime.Children.Add(DPV_LowTime);

            Rectangle spacer = new Rectangle { Width = 40 };
            DPV_innerStackPanelFinalVoltage.Children.Add(spacer);
            DPV_VerticalPanel2.Children.Add(CurrentInLog);
            DPV_VerticalPanel2.Children.Add(DPV_SendParameters);
            CurrentInLog.Checked += CheckBox_Checked;
            CurrentInLog.Unchecked += CheckBox_Unchecked;
        }
        #endregion
        #endregion

        #region Normal Pulse Voltammetry
        private void NPV_SendMeasureParameters(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(NPV_PulseTime.Text) && !string.IsNullOrEmpty(NPV_VoltageStep.Text)
                && !string.IsNullOrEmpty(NPV_InitialVoltage.Text) && !string.IsNullOrEmpty(NPV_FinalVoltage.Text) &&
                !string.IsNullOrEmpty(NPV_LowTime.Text))
            {
                MessageBox.Show("Starting measure.");
                NPVVoltagePointsArrayCalculator(float.Parse(NPV_VoltageStep.Text), float.Parse(NPV_InitialVoltage.Text),
                    float.Parse(NPV_FinalVoltage.Text), float.Parse(NPV_LowTime.Text), float.Parse(NPV_PulseTime.Text));
                IsInMeasure = true;
                _serialPort.Write("3" + NPV_PulseTime.Text + NPV_VoltageStep.Text + NPV_InitialVoltage.Text
                    + NPV_FinalVoltage.Text + NPV_LowTime.Text);
            }
            else MessageBox.Show("Fill all the parameters to make the measure.", "Error");
        }
        private void NPVVoltagePointsArrayCalculator(float VoltageStep, float InitialVoltage, float FinalVoltage,
            float LowTime, float PulseTime)
        {
            VoltageStep = VoltageStep * 0.001f;
            VoltagePoints = InitialVoltage.ToString();
            float lastVoltage = InitialVoltage;

            while (lastVoltage < FinalVoltage)
            {
                lastVoltage = lastVoltage + VoltageStep;
                if (lastVoltage > FinalVoltage) break;
                VoltagePoints = VoltagePoints + "," + lastVoltage.ToString();
                VoltagePoints = VoltagePoints + "," + InitialVoltage.ToString();
            }
            VoltagePoints = VoltagePoints + "," + FinalVoltage.ToString();
            //_serialPort.Write(VoltagePoints);
            VoltagePointsFloat = ConvertStringToFloatArray(VoltagePoints);
        }

        #region NPV Config Panel Design
        private void NPV_CreateConfigPanelItems()
        {
            NPV_VerticalPanel1 = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 0, 10)
            };
            NPV_VerticalPanel2 = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 0, 10)
            };
            NPV_innerStackPanelPulseTime = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            NPV_innerStackPanelVoltageStep = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            NPV_innerStackPanelInitialVoltage = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            NPV_innerStackPanelFinalVoltage = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            NPV_innerStackPanelLowTime = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };

            NPV_PulseTimeText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Pulse Time",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            NPV_PulseTime = new TextBox
            {
                Width = 50,
                Height = 20
            };
            NPV_VoltageStepText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Voltage Step",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            NPV_VoltageStep = new TextBox
            {
                Width = 50,
                Height = 20
            };
            NPV_InitialVoltageText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Initial Voltage",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            NPV_InitialVoltage = new TextBox
            {
                Width = 50,
                Height = 20
            };
            NPV_FinalVoltageText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Final Voltage",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            NPV_FinalVoltage = new TextBox
            {
                Width = 50,
                Height = 20
            };
            NPV_LowTimeText = new TextBox
            {
                Width = 80,
                Height = 20,
                Text = "Low Time",
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            NPV_LowTime = new TextBox
            {
                Width = 50,
                Height = 20
            };
            NPV_SendParameters = new Button
            {
                Width = 80,
                Height = 20,
                Content = "Start Measure"
            };
            CurrentInLog = new CheckBox
            {
                Content = "Plot Current in Log",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 0)
            };
            NPV_SendParameters.Click += NPV_SendMeasureParameters;
        }
        private void NPV_InitializeConfigPanelItems()
        {
            configPanel.Children.Add(NPV_VerticalPanel1);
            configPanel.Children.Add(NPV_VerticalPanel2);

            NPV_VerticalPanel1.Children.Add(NPV_innerStackPanelPulseTime);
            NPV_innerStackPanelPulseTime.Children.Add(NPV_PulseTimeText);
            NPV_innerStackPanelPulseTime.Children.Add(NPV_PulseTime);

            NPV_VerticalPanel1.Children.Add(NPV_innerStackPanelVoltageStep);
            NPV_innerStackPanelVoltageStep.Children.Add(NPV_VoltageStepText);
            NPV_innerStackPanelVoltageStep.Children.Add(NPV_VoltageStep);

            NPV_VerticalPanel1.Children.Add(NPV_innerStackPanelInitialVoltage);
            NPV_innerStackPanelInitialVoltage.Children.Add(NPV_InitialVoltageText);
            NPV_innerStackPanelInitialVoltage.Children.Add(NPV_InitialVoltage);

            NPV_VerticalPanel1.Children.Add(NPV_innerStackPanelFinalVoltage);
            NPV_innerStackPanelFinalVoltage.Children.Add(NPV_FinalVoltageText);
            NPV_innerStackPanelFinalVoltage.Children.Add(NPV_FinalVoltage);

            NPV_VerticalPanel2.Children.Add(NPV_innerStackPanelLowTime);
            NPV_innerStackPanelLowTime.Children.Add(NPV_LowTimeText);
            NPV_innerStackPanelLowTime.Children.Add(NPV_LowTime);

            Rectangle spacer = new Rectangle { Width = 40 };
            NPV_innerStackPanelFinalVoltage.Children.Add(spacer);
            NPV_VerticalPanel2.Children.Add(CurrentInLog);
            NPV_VerticalPanel2.Children.Add(NPV_SendParameters);
            CurrentInLog.Checked += CheckBox_Checked;
            CurrentInLog.Unchecked += CheckBox_Unchecked;
        }
        #endregion
        #endregion

        #region General purpose methods / Chart methods
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
                        CV_CreateConfigPanelItems();
                        CV_InitializeConfigPanelItems();
                        break;
                    case "Differential Pulse Voltammetry":
                        CurrentSeries.Clear();
                        configPanel.Children.Clear();
                        DPV_CreateConfigPanelItems();
                        DPV_InitializeConfigPanelItems();
                        break;
                    case "Normal Pulse Voltammetry":
                        CurrentSeries.Clear();
                        configPanel.Children.Clear();
                        NPV_CreateConfigPanelItems();
                        NPV_InitializeConfigPanelItems();
                        break;
                    default:

                        break;

                }
            }
        }

        private void CurrentPointsPlotter(object sender, SerialDataReceivedEventArgs e)
        {
            if (IsInMeasure)
            {
                Thread.Sleep(1000);
                string input = _serialPort.ReadExisting();
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                MeasuredCurrent = ConvertStringToFloatArray(input);
                Array.Resize(ref MeasuredCurrent, MeasuredCurrent.Length - 1);
                Console.WriteLine("len measuredcurretn, votlagePoints");
                Console.WriteLine(MeasuredCurrent.Length);
                Console.WriteLine(VoltagePointsFloat.Length);

                /*for (int i = 0; i< MeasuredCurrent.Length; i++)
                {
                    Console.Write(MeasuredCurrent[i]);
                    Console.Write(",");
                }
                Console.WriteLine("");
                for (int i = 0; i < VoltagePointsFloat.Length; i++)
                {
                    Console.Write(VoltagePointsFloat[i]);
                    Console.Write(",");
                }*/

                Dispatcher.Invoke(() =>
                {
                    if (ivSeries == null)
                    {
                        ivSeries = new LineSeries
                        {
                            Title = "IV Curve",
                            Values = new ChartValues<ObservablePoint>(),
                            PointGeometry = DefaultGeometries.Circle,
                            PointGeometrySize = 5
                        };
                    }

                    if (LogIsChecked)
                    {
                        for (int i = 0; i < MeasuredCurrent.Length - 1; i++)
                        {
                            if (NotFirstData)
                            {
                                ivSeries.Values.Add(new ObservablePoint(VoltagePointsFloat[VoltageArrayIndexOffset], Math.Log10(MeasuredCurrent[i])));
                            }
                            else
                            {
                                ivSeries.Values.Add(new ObservablePoint(VoltagePointsFloat[i], Math.Log10(MeasuredCurrent[i])));
                            }
                            VoltageArrayIndexOffset++;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < MeasuredCurrent.Length - 1; i++)
                        {
                            if (NotFirstData)
                            {
                                ivSeries.Values.Add(new ObservablePoint(VoltagePointsFloat[VoltageArrayIndexOffset], MeasuredCurrent[i]));
                            }
                            else
                            {
                                ivSeries.Values.Add(new ObservablePoint(VoltagePointsFloat[i], MeasuredCurrent[i]));
                            }
                            VoltageArrayIndexOffset++;
                        }
                    }
                    CurrentSeries.Add(ivSeries);
                    NotFirstData = true;
                });
                Thread.Sleep(1200);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            LogIsChecked = true;
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            LogIsChecked = false;
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
        #region Export CSV
        void SaveToCsv(object sender, RoutedEventArgs e)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string FolderName = "SMUExport";

            string FolderPath = System.IO.Path.Combine(documentsPath, FolderName);

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            
            string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmm");
            string fileName = $"{timestamp}.csv";
            string filePath = System.IO.Path.Combine(FolderPath, fileName);

            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("current,voltage");

            int rowCount = Math.Min(MeasuredCurrent.Length, VoltagePointsFloat.Length);

            if (LogIsChecked)
            {
                for (int i = 0; i < rowCount; i++)
                {
                    csvContent.AppendLine($"{Math.Log10(MeasuredCurrent[i])},{VoltagePointsFloat[i]}");
                }
            }
            else
            {
                for (int i = 0; i < rowCount; i++)
                {
                    csvContent.AppendLine($"{MeasuredCurrent[i]},{VoltagePointsFloat[i]}");
                }
            }

            File.WriteAllText(filePath, csvContent.ToString());

            MessageBox.Show("CSV file created in Documents/SMUExport.");
        }
        #endregion
        #region Export Chart
        private void ExportGraphPNG(object sender, RoutedEventArgs e)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string FolderName = "SMUExport";

            string FolderPath = System.IO.Path.Combine(documentsPath, FolderName);

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmm");
            string fileName = $"{timestamp}.png";
            string filePath = System.IO.Path.Combine(FolderPath, fileName);

            SaveChartAsPng(Chart, filePath);
            MessageBox.Show("PNG file created in Documents/SMUExport.");
        }

        static void SaveChartAsPng(FrameworkElement chart, string filePath)
        {
            var encoder = new PngBitmapEncoder();
            EncodeVisual(chart, encoder);

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                encoder.Save(stream);
            }
        }

        static void EncodeVisual(FrameworkElement visual, BitmapEncoder encoder)
        {
            if (visual == null)
                throw new ArgumentNullException(nameof(visual));

            var size = new Size(visual.ActualWidth, visual.ActualHeight);

            var renderBitmap = new RenderTargetBitmap(
                (int)size.Width, (int)size.Height,
                96, 96, System.Windows.Media.PixelFormats.Pbgra32);

            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                var visualBrush = new VisualBrush(visual);
                context.DrawRectangle(visualBrush, null, new Rect(new Point(), size));
            }

            renderBitmap.Render(drawingVisual);
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
        }
        #endregion
        #endregion
    }
}
