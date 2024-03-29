﻿using HealthCareCenter.Core;
using HealthCareCenter.Core.Appointments.Models;
using HealthCareCenter.Core.Appointments.Repository;
using HealthCareCenter.Core.Appointments.Services;
using HealthCareCenter.Core.Appointments.Services.Priority;
using HealthCareCenter.Core.HealthRecords;
using HealthCareCenter.Core.Patients.Services;
using HealthCareCenter.Core.Rooms.Repositories;
using HealthCareCenter.Core.Rooms.Services;
using HealthCareCenter.Core.Users;
using HealthCareCenter.GUI.Patient.AppointmentCRUD.ViewModels;
using HealthCareCenter.GUI.Patient.SharedCommands;
using HealthCareCenter.GUI.Patient.SharedViewModels;
using System;
using System.Collections.Generic;
using System.Windows;

namespace HealthCareCenter.GUI.Patient.AppointmentCRUD.Commands
{
    internal class PriorityScheduleAppointmentCommand : CommandBase
    {
        private void SetupPriorityNotFound()
        {
            _ = MessageBox.Show("Couldn't find appointments by priority, showing 3 closest to priority",
                    "My App", MessageBoxButton.OK, MessageBoxImage.Information);

            List<Appointment> similarToPriority = _prioritySearchService.GetAppointmentsSimilarToPriorites(
                _viewModel.IsDoctorPriority, _viewModel.ChosenDoctor.DoctorID, _viewModel.Patient.HealthRecordID,
                _viewModel.ChosenDate, _viewModel.StartRange, _viewModel.EndRange);

            if (similarToPriority.Count == 0)
            {
                _ = MessageBox.Show("Couldn't find appointments similar to priority", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                return;
            }

            List<PriorityNotFoundChoiceViewModel> alternativeChoices = new List<PriorityNotFoundChoiceViewModel>();
            foreach (Appointment appointment in similarToPriority)
            {
                alternativeChoices.Add(new PriorityNotFoundChoiceViewModel(appointment));
            }

            _viewModel.PriorityNotFoundChoices = alternativeChoices;
        }

        private void PriorityFound()
        {
            if (_viewModel.ChosenDoctor == null)
            {
                _ = MessageBox.Show("Doctor not chosen", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_viewModel.ChosenDate.Date.CompareTo(DateTime.Now.Date) <= 0)
            {
                _ = MessageBox.Show("Invalid date", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(_viewModel.StartRange < _viewModel.EndRange))
            {
                _ = MessageBox.Show("Invalid range", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Appointment newAppointment = _prioritySearchService.GetPriorityAppointment(
                _viewModel.IsDoctorPriority, Convert.ToInt32(_viewModel.ChosenDoctor.DoctorID), _viewModel.Patient.HealthRecordID,
                _viewModel.ChosenDate, _viewModel.StartRange, _viewModel.EndRange);

            if (newAppointment == null)
            {
                SetupPriorityNotFound();
                return;
            }

            string appointmentDetails = "Doctor: " + newAppointment.DoctorID + ", Schedule: " + newAppointment.ScheduledDate.ToString("g");
            _ = MessageBox.Show("Appointment found: " + appointmentDetails);

            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Schedule appointment?", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                bool passed = _appointmentService.Schedule(newAppointment, true);
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
            }
        }

        private void PriorityNotFound()
        {
            if (_viewModel.PriorityNotFoundChoice == null)
            {
                _ = MessageBox.Show("Appointment not chosen", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int doctorID = _viewModel.PriorityNotFoundChoice.DoctorID;
            DateTime scheduleDate = _viewModel.PriorityNotFoundChoice.AppointmentDate;
            int hospitalRoomID = _hospitalRoomService.GetAvailableRoomID(scheduleDate, RoomType.Checkup);
            if (hospitalRoomID == -1)
            {
                MessageBox.Show("No available rooms", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                return;
            }

            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Schedule appointment?", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                bool passed = _appointmentService.Schedule(
                    scheduleDate, doctorID, _viewModel.Patient.HealthRecordID, hospitalRoomID);
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
            }
        }

        public override void Execute(object parameter)
        {
            if (_viewModel.PriorityNotFoundChoices.Count == 0)
            {
                PriorityFound();
            }
            else
            {
                PriorityNotFound();
            }

        }

        private readonly PrioritySchedulingViewModel _viewModel;
        private readonly NavigationStore _navigationStore;
        private readonly IAppointmentPrioritySearchService _prioritySearchService;
        private readonly IAppointmentService _appointmentService;
        private readonly IHospitalRoomService _hospitalRoomService;

        public PriorityScheduleAppointmentCommand(
            PrioritySchedulingViewModel viewModel, 
            NavigationStore navigationStore,
            IAppointmentPrioritySearchService prioritySearchService,
            IAppointmentService appointmentService,
            IHospitalRoomService hospitalRoomService)
        {
            _viewModel = viewModel;
            _navigationStore = navigationStore;
            _prioritySearchService = prioritySearchService;
            _appointmentService = appointmentService;
            _hospitalRoomService = hospitalRoomService;
        }
    }
}
