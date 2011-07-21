﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Cyclops.MainApplication.ViewModel;

namespace Cyclops.MainApplication.View.Options
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView 
    {
        public SettingsView()
        {
            DataContext = new SettingsViewModel(() => { }, () => { });
            InitializeComponent();
        }

        public static void ShowSettings()
        {
        }
    }
}
