﻿using System;
using System.Windows;
using HealthCareCenter.Core.Appointments.Services;
using HealthCareCenter.Core;
using HealthCareCenter.Core.Notifications.Services;
using HealthCareCenter.Core.Notifications.Repositories;
using HealthCareCenter.Core.Patients;
using HealthCareCenter.Core.Appointments.Urgent.Services;
using HealthCareCenter.Core.Appointments.Urgent.Controllers;
using HealthCareCenter.Core.Appointments.Urgent.DTO;
using HealthCareCenter.Core.Appointments.Repository;
using HealthCareCenter.Core.HealthRecords;
using HealthCareCenter.Core.Medicine.Repositories;
using HealthCareCenter.Core.Medicine.Services;
using HealthCareCenter.Core.Patients.Services;
using HealthCareCenter.Core.Rooms.Services;
using HealthCareCenter.Core.Rooms.Repositories;
using HealthCareCenter.Core.Users;
using HealthCareCenter.Core.Users.Services;

namespace HealthCareCenter.Secretary
{
    /// <summary>
    /// Interaction logic for ScheduleUrgentAppointmentWindow.xaml
    /// </summary>
    public partial class ScheduleUrgentAppointmentWindow : Window
    {
        private readonly Patient _patient;
        private OccupiedAppointmentsDTO _info;

        private readonly ScheduleUrgentAppointmentController _controller;

        public ScheduleUrgentAppointmentWindow()
        {
            InitializeComponent();
        }

        public ScheduleUrgentAppointmentWindow(Patient patient)
        {
            _patient = patient;
            _info = new OccupiedAppointmentsDTO();
            BaseUrgentAppointmentService service = new UrgentAppointmentService(
                new TermsService(
                    new AppointmentRepository()),
                new NotificationService(
                    new NotificationRepository(),
                    new HealthRecordService(
                        new HealthRecordRepository()),
                    new MedicineInstructionService(
                        new MedicineInstructionRepository()),
                    new MedicineService(
                        new MedicineRepository()),
                    new HospitalRoomService(
                        new AppointmentRepository(),
                        new HospitalRoomForRenovationService(
                            new HospitalRoomForRenovationRepository()),
                        new HospitalRoomRepository())),
                new AppointmentRepository(),
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
                new HospitalRoomService(
                    new AppointmentRepository(),
                    new HospitalRoomForRenovationService(
                        new HospitalRoomForRenovationRepository()),
                    new HospitalRoomRepository()),
                new HospitalRoomRepository(),
                new UserRepository(),
                new DoctorService(
                    new DoctorSearchService(
                        new UserRepository()),
                    new UserRepository())) { UrgentInfo = _info };
            _controller = new ScheduleUrgentAppointmentController(
                service,
                new DoctorService(
                    new DoctorSearchService(
                        new UserRepository()),
                    new UserRepository()));

            InitializeComponent();

            try
            {
                doctorTypesListBox.ItemsSource = _controller.GetTypesOfDoctors();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void ScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            if (!EnteredData())
                return;

            TryScheduling();
        }

        private void TryScheduling()
        {
            AppointmentType type = GetSelectedAppointmentType();
            try
            {
                if (!_controller.TryScheduling(type, doctorTypesListBox.SelectedItem.ToString(), _patient))
                {
                    OpenPostponingWindow(type);
                }
                else
                {
                    MessageBox.Show("Successfully scheduled urgent appointment.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show(ex.StackTrace);
                return;
            }
            Close();
        }

        private AppointmentType GetSelectedAppointmentType()
        {
            AppointmentType type = AppointmentType.Checkup;
            if ((bool)operationRadioButton.IsChecked)
                type = AppointmentType.Operation;
            return type;
        }

        private void OpenPostponingWindow(AppointmentType type)
        {
            if (_info.OccupiedAppointments.Count == 0)
            {
                MessageBox.Show("No available term was found in the next 2 hours. Unfortunately, there are no terms to postpone at this time neither.");
                return;
            }
            MessageBox.Show("No available term was found in the next 2 hours. You can, however, postpone an occupied term in the next window.");
            OccupiedAppointmentsWindow window = new OccupiedAppointmentsWindow(_patient, type, _info);
            window.ShowDialog();
            Close();
        }

        private bool EnteredData()
        {
            if (doctorTypesListBox.SelectedItem == null)
            {
                MessageBox.Show("You must select a type of doctor first.");
                return false;
            }
            if (!(bool)checkupRadioButton.IsChecked && !(bool)operationRadioButton.IsChecked)
            {
                MessageBox.Show("You must select a type of appointment first.");
                return false;
            }
            return true;
        }
    }
}
