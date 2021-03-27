﻿using DirectShowLib;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.ViewModels;
using System;
using System.Windows;

namespace LagoVista.PickAndPlace.App
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
	{

        MachineSettings _settings;

        public SettingsWindow(IMachine machine, MachineSettings settings, int index = 0)
		{
            _settings = settings;

            DataContext = new SettingsViewModel(machine, _settings);
            
            var cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            var idx = 0;

            ViewModel.Cameras.Add(new Models.Camera()
            {
                Id = "-1",
                Name = "none",
                CameraIndex = -1
            });

            foreach (var camera in cameras)
            {
                ViewModel.Cameras.Add(new Models.Camera()
                {
                    Id = camera.DevicePath,
                    Name = camera.Name,
                    CameraIndex = idx++
                });
            }

            InitializeComponent();

            Tabs.SelectedIndex = index;
		}
        public SettingsViewModel ViewModel
        {
            get { return DataContext as SettingsViewModel; }
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(_settings.MachineName))
            {
                MessageBox.Show("Machine Name is a Required Field");
                Tabs.TabIndex = 0;
                MachineName.Focus();
                return;
            }

            if (String.IsNullOrEmpty(ViewModel.MachineType))
            {
                MessageBox.Show("Machine Type is a Required Field");
                Tabs.TabIndex = 0;
                MachineName.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
