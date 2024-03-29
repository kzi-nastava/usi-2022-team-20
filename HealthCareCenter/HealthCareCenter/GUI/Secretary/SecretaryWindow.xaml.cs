﻿using System;
using System.Collections.Generic;
using System.Windows;
using HealthCareCenter.Core.Equipment.Services;
using HealthCareCenter.Core.Notifications;
using HealthCareCenter.Core.Notifications.Services;
using HealthCareCenter.Core.Rooms.Repositories;
using HealthCareCenter.Core.Users;
using HealthCareCenter.Core.Users.Models;

namespace HealthCareCenter.Secretary
{
    public partial class SecretaryWindow : Window
    {
        private readonly Core.Users.Models.Secretary _signedUser;
        private readonly SecretaryController _controller;
        private readonly IDynamicEquipmentService _dynamicEquipmentService;
        private readonly INotificationService _notificationService;

        public SecretaryWindow(User user, INotificationService notificationService, IDynamicEquipmentService dynamicEquipmentService)
        {
            _signedUser = (Core.Users.Models.Secretary)user;
            _controller = new SecretaryController(notificationService);
            _dynamicEquipmentService = dynamicEquipmentService;
            _notificationService = notificationService;

            InitializeComponent();
            DisplayNotifications();
        }

        private void DisplayNotifications()
        {
            List<Notification> notifications;
            try
            {
                notifications = _controller.GetNotifications(_signedUser);
            }
            catch (Exception)
            {
                return;
            }
            MessageBox.Show("You have new notifications.");
            foreach (Notification notification in notifications)
            {
                MessageBox.Show(notification.Message);
            }
        }

        private void PatientButton_Click(object sender, RoutedEventArgs e)
        {
            PatientManipulationWindow window = new PatientManipulationWindow(new UserRepository());
            window.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LoginWindow window = new LoginWindow();
            window.Show();
        }

        private void EquipmentDistributionButton_Click(object sender, RoutedEventArgs e)
        {
            DistributeDynamicEquipmentWindow window = new DistributeDynamicEquipmentWindow(
                _dynamicEquipmentService,
                new StorageRepository(),
                new HospitalRoomRepository());
            window.ShowDialog();
        }
        
        private void EquipmentAcquisitionButton_Click(object sender, RoutedEventArgs e)
        {
            DynamicEquipmentRequestWindow window = new DynamicEquipmentRequestWindow(_signedUser, _dynamicEquipmentService);
            window.ShowDialog();
        }

        private void VacationButton_Click(object sender, RoutedEventArgs e)
        {
            VacationRequestsWindow window = new VacationRequestsWindow(_notificationService);
            window.ShowDialog();
        }
    }
}
