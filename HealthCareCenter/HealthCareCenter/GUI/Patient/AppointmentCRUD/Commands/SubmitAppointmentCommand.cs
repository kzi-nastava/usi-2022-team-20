﻿using HealthCareCenter.Core;
using HealthCareCenter.Core.Appointments.Repository;
using HealthCareCenter.Core.Appointments.Services;
using HealthCareCenter.Core.HealthRecords;
using HealthCareCenter.Core.Patients.Services;
using HealthCareCenter.Core.Rooms.Repositories;
using HealthCareCenter.Core.Rooms.Services;
using HealthCareCenter.Core.Users;
using HealthCareCenter.GUI.Patient.AppointmentCRUD.ViewModels;
using HealthCareCenter.GUI.Patient.SharedCommands;
using HealthCareCenter.GUI.Patient.SharedViewModels;
using System;
using System.Windows;

namespace HealthCareCenter.GUI.Patient.AppointmentCRUD.Commands
{
    internal class SubmitAppointmentCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            DateTime scheduleDate = Convert.ToDateTime(
                _viewModel.ChosenDate.ToString("d") + " " + _viewModel.ChosenTerm.ToString());
            int hospitalRoomID = _hospitalRoomService.GetAvailableRoomID(scheduleDate, RoomType.Checkup);

            if (!IsValid(scheduleDate, hospitalRoomID))
            {
                return;
            }

            if (_viewModel.ChosenAppointment == null)
            {
                ScheduleAppointment(scheduleDate, hospitalRoomID);
            }
            else
            {
                ModifyAppointment(scheduleDate, hospitalRoomID);
            }

        }

        private readonly NavigationStore _navigationStore;
        private readonly AppointmentFormViewModel _viewModel;
        private readonly IAppointmentService _appointmentService;
        private readonly IHospitalRoomService _hospitalRoomService;

        public SubmitAppointmentCommand(
            NavigationStore navigationStore, 
            AppointmentFormViewModel viewModel,
            IAppointmentService appointmentService,
            IHospitalRoomService hospitalRoomService)
        {
            _navigationStore = navigationStore;
            _viewModel = viewModel;
            _appointmentService = appointmentService;
            _hospitalRoomService = hospitalRoomService;
        }

        private bool IsValid(DateTime scheduleDate, int hospitalRoomID)
        {
            if (_viewModel.ChosenDate.Date.CompareTo(DateTime.Now.Date) <= 0)
            {
                _ = MessageBox.Show("Invalid date", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_viewModel.ChosenDoctor == null)
            {
                _ = MessageBox.Show("Doctor not chosen", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_viewModel.ChosenTerm == null)
            {
                _ = MessageBox.Show("No schedule selected", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!_appointmentService.IsAvailable(scheduleDate, _viewModel.ChosenDoctor.DoctorID))
            {
                _ = MessageBox.Show("That schedule is unavailable", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (hospitalRoomID == -1)
            {
                _ = MessageBox.Show("No available room in that schedule", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ScheduleAppointment(DateTime scheduleDate, int hospitalRoomID)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Schedule appointment?", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                bool passed = _appointmentService.Schedule(
                    scheduleDate, _viewModel.ChosenDoctor.DoctorID, _viewModel.Patient.HealthRecordID, hospitalRoomID);

                _navigationStore.CurrentViewModel = new MyAppointmentsViewModel(
                    new AppointmentService(
                        new AppointmentRepository(),
                        new AppointmentChangeRequestRepository(),
                        new AppointmentChangeRequestService(
                            new AppointmentRepository(),
                            new AppointmentChangeRequestRepository(),
                            new HospitalRoomService(
                                new AppointmentRepository(),
                                new HospitalRoomForRenovationService(
                                    new HospitalRoomForRenovationRepository()),
                                new HospitalRoomRepository()),
                            new UserRepository()),
                        new PatientService(
                            new AppointmentRepository(),
                            new AppointmentChangeRequestRepository(),
                            new HealthRecordRepository(),
                            new HealthRecordService(
                                new HealthRecordRepository()),
                            new PatientEditService(
                                new HealthRecordRepository(),
                                new UserRepository()),
                            new UserRepository()),
                        new HospitalRoomService(
                            new AppointmentRepository(),
                            new HospitalRoomForRenovationService(
                                new HospitalRoomForRenovationRepository()),
                            new HospitalRoomRepository()),
                        new HospitalRoomRepository()),
                    _viewModel.Patient,
                    _navigationStore);

                if (!passed)
                {
                    _ = MessageBox.Show("Trolling limit reached! This account will be blocked", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _viewModel.Patient.IsBlocked = true;
                    _viewModel.Patient.BlockedBy = Blocker.System;

                    LoginWindow win = new LoginWindow();
                    win.Show();
                    Application.Current.Windows[0].Close();

                    return;
                }
            }
        }

        private void ModifyAppointment(DateTime scheduleDate, int hospitalRoomID)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Schedule appointment?", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                if (_appointmentService.ShouldSendToSecretary(_viewModel.ChosenAppointment.AppointmentDate))
                {
                    _ = MessageBox.Show("Since there are less than 2 days until this appointment starts, a request will be sent to the secretary",
                        "My App", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                bool passed = _appointmentService.Edit(
                    scheduleDate, _viewModel.ChosenAppointment.AppointmentDate, _viewModel.ChosenAppointment.AppointmentID,
                    _viewModel.ChosenDoctor.DoctorID, _viewModel.Patient.ID, hospitalRoomID);

                _navigationStore.CurrentViewModel = new MyAppointmentsViewModel(
                    new AppointmentService(
                        new AppointmentRepository(),
                        new AppointmentChangeRequestRepository(),
                        new AppointmentChangeRequestService(
                            new AppointmentRepository(),
                            new AppointmentChangeRequestRepository(),
                            new HospitalRoomService(
                                new AppointmentRepository(),
                                new HospitalRoomForRenovationService(
                                    new HospitalRoomForRenovationRepository()),
                                new HospitalRoomRepository()),
                            new UserRepository()),
                        new PatientService(
                            new AppointmentRepository(),
                            new AppointmentChangeRequestRepository(),
                            new HealthRecordRepository(),
                            new HealthRecordService(
                                new HealthRecordRepository()),
                            new PatientEditService(
                                new HealthRecordRepository(),
                                new UserRepository()),
                            new UserRepository()),
                        new HospitalRoomService(
                            new AppointmentRepository(),
                            new HospitalRoomForRenovationService(
                                new HospitalRoomForRenovationRepository()),
                            new HospitalRoomRepository()),
                        new HospitalRoomRepository()),
                    _viewModel.Patient,
                    _navigationStore);

                if (!passed)
                {
                    _ = MessageBox.Show("Trolling limit reached! This account will be blocked", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _viewModel.Patient.IsBlocked = true;
                    _viewModel.Patient.BlockedBy = Blocker.System;

                    LoginWindow win = new LoginWindow();
                    win.Show();
                    Application.Current.Windows[0].Close();

                    return;
                }
            }
        }
    }
}
