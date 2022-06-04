﻿using HealthCareCenter.PatientGUI.Models;
using HealthCareCenter.PatientGUI.Stores;
using HealthCareCenter.PatientGUI.ViewModels;
using HealthCareCenter.Service;
using System;
using System.Windows;

namespace HealthCareCenter.PatientGUI.Commands
{
    internal class CancelAppointmentCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            if (_viewModel.ChosenAppointment == null)
            {
                _ = MessageBox.Show("Appointment not chosen", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Schedule appointment?", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                if (AppointmentService.ShouldSendToSecretary(_viewModel.ChosenAppointment.AppointmentDate))
                {
                    _ = MessageBox.Show("Since there are less than 2 days until this appointment starts, a request will be sent to the secretary",
                        "My App", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                bool passed = AppointmentService.CancelAppointment(
                    _viewModel.ChosenAppointment.AppointmentID, _viewModel.Patient.ID, _viewModel.ChosenAppointment.AppointmentDate);

                if (!passed)
                {
                    _ = MessageBox.Show("Trolling limit reached! This account will be blocked", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _viewModel.Patient.IsBlocked = true;
                    _viewModel.Patient.BlockedBy = Enums.Blocker.System;

                    LoginWindow win = new LoginWindow();
                    win.Show();
                    Application.Current.Windows[0].Close();
                    
                    return;
                }

                _navigationStore.CurrentViewModel = new MyAppointmentsViewModel(_navigationStore, _viewModel.Patient);
            }
        }

        private readonly MyAppointmentsViewModel _viewModel;
        private readonly NavigationStore _navigationStore;

        public CancelAppointmentCommand(MyAppointmentsViewModel viewModel, NavigationStore navigationStore)
        {
            _viewModel = viewModel;
            _navigationStore = navigationStore;
        }
    }
}
