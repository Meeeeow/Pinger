﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Pinger.Model;
using Pinger.Controller;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Pinger
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        // MainWindow
        private MainWindowViewModel ctx;
        private PingerController controller;
        private int barCount = 30;
        
        private ObservableCollection<double> rttResults;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            ctx = (MainWindowViewModel)DataContext;
            controller = new PingerController(ctx.PingInterval, 30);
            rttResults = new ObservableCollection<double>();
            rttResults.CollectionChanged += RttPoolChangeHandler;

            UpdateCanvas();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (controller.Status != PingerStatus.Stopped)
            {
                controller.Stop();
                ctx.ActionButtonText = "Start";
                return;
            } else
            {
                rttResults.Clear();
                ctx.ActionButtonText = "Stop";
            }

            switch (ctx.SelectedPingProtocol)
            {
                case PingProtocol.ICMP:
                    controller.Setup(HostInput.Text, ctx.PingInterval);
                    controller.Start(ref rttResults);
                    break;
                case PingProtocol.TCP:
                    controller.Setup(HostInput.Text, Convert.ToUInt16(PortInput.Text), 3000, ctx.PingInterval);
                    controller.Start(ref rttResults);
                    break;
                default:
                    throw new Exception("Unknown protocol");
            }
        }

        private void RttPoolChangeHandler (object sender, NotifyCollectionChangedEventArgs args)
        {
            UpdateCanvas();
        }

        private void UpdateCanvas()
        {
            Dispatcher.Invoke(() =>
            {
                var c = ResultCanvas;
                c.Children.Clear();

                // Set title bar
                c.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9F9F9"));
                var text = new TextBlock
                {
                    Text = $"Latency graph ({barCount}, {barCount * (ctx.PingInterval / 1000f)}s)",
                    Foreground = Brushes.Gray
                };
                Canvas.SetTop(text, 0);
                Canvas.SetLeft(text, 0);
                c.Children.Add(text);

                // Draw bar
                var cHeight = c.ActualHeight - 35;
                var cWidth = c.ActualWidth;
                var barWidth = cWidth / barCount;
                for (var i = 0; i < rttResults.Count; i++)
                {
                    var currentRtt = rttResults[i];
                    var currentRttInt = (uint)Math.Round(currentRtt);
                    var barHeight = (currentRtt / (double)Math.Max(rttResults.Max(), 1)) * cHeight;
                    var barRight = i * barWidth;

                    var bar = new Rectangle
                    {
                        Fill = Brushes.Pink,
                        Height = barHeight,
                        Width = barWidth - 5
                    };
                    Canvas.SetBottom(bar, 0);
                    Canvas.SetRight(bar, barRight);

                    var rttTextLength = currentRttInt.ToString().Length;
                    // Latency text on per bar
                    var rttText = new TextBlock
                    {
                        Text = (currentRtt > 0 && currentRtt < 1) ? "<1" : currentRttInt.ToString(),
                        FontWeight = FontWeights.Bold,
                        // Maxium font size is the half of bar width
                        FontSize = barWidth / (rttTextLength > 2 ? rttTextLength : 2),
                        Width = barWidth - 5,
                        TextAlignment = TextAlignment.Center
                    };
                    Canvas.SetBottom(rttText, barHeight + 2);
                    Canvas.SetRight(rttText, barRight);
                    c.Children.Add(rttText);
                    c.Children.Add(bar);
                }

            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (controller.Status != PingerStatus.Stopped)
            {
                controller.Stop();
            }
        }

        private void PreviewTextInput_NumberOnly(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void HostInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"[^(0-9|a-z|A-Z|\.)]");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
