﻿using HealthCareCenter.Core.Appointments.Models;
using HealthCareCenter.Core.Appointments.Repository;
using HealthCareCenter.Core.Appointments.Services;
using HealthCareCenter.Core.HealthRecords;
using HealthCareCenter.Core.Patients.Services;
using HealthCareCenter.Core.Rooms.Repositories;
using HealthCareCenter.Core.Rooms.Services;
using HealthCareCenter.Core.Users;
using HealthCareCenter.GUI.Patient.Profile.Commands;
using HealthCareCenter.GUI.Patient.SharedViewModels;
using System.Collections.Generic;
using System.Windows.Input;

namespace HealthCareCenter.GUI.Patient.Profile.ViewModels
{
    internal class MyHealthRecordViewModel : ViewModelBase
    {
        public Core.Patients.Patient Patient;
        public HealthRecord HealthRecord;

        private List<AppointmentViewModel> _appointments;
        public List<AppointmentViewModel> Appointments
        {
            get => _appointments;
            set
            {
                _appointments = value;
                OnPropertyChanged(nameof(Appointments));
            }
        }

        private AppointmentViewModel _chosenAppointment;
        public AppointmentViewModel ChosenAppointment
        {
            get => _chosenAppointment;
            set
            {
                _chosenAppointment = value;
                OnPropertyChanged(nameof(ChosenAppointment));
            }
        }

        private string _patientHeight;
        public string PatientHeight
        {
            get => _patientHeight;
            set
            {
                _patientHeight = value;
                OnPropertyChanged(nameof(PatientHeight));
            }
        }

        private string _patientWeight;
        public string PatientWeight
        {
            get => _patientWeight;
            set
            {
                _patientWeight = value;
                OnPropertyChanged(nameof(PatientWeight));
            }
        }

        private string _patientAllergens;
        public string PatientAllergens
        {
            get => _patientAllergens;
            set
            {
                _patientAllergens = value;
                OnPropertyChanged(PatientAllergens);
            }
        }

        private string _patientPreviousDiseases;
        public string PatientPreviousDiseases
        {
            get => _patientPreviousDiseases;
            set
            {
                _patientPreviousDiseases = value;
                OnPropertyChanged(nameof(PatientPreviousDiseases));
            }
        }

        private readonly List<string> _sortCriteria;
        public List<string> SortCriteria => _sortCriteria;

        private string _chosenSortCriteria;
        public string ChosenSortCriteria
        {
            get => _chosenSortCriteria;
            set
            {
                _chosenSortCriteria = value;
                OnPropertyChanged(nameof(ChosenSortCriteria));
            }
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                OnPropertyChanged(nameof(SearchKeyword));
            }
        }

        private string _anamnesisInfo;
        public string AnamnesisInfo
        {
            get => _anamnesisInfo;
            set
            {
                _anamnesisInfo = value;
                OnPropertyChanged(nameof(AnamnesisInfo));
            }
        }

        public ICommand SortAppointments { get; }
        public ICommand SearchAppointments { get; }
        public ICommand ShowAnamnesis { get; }

        public MyHealthRecordViewModel(
            IAppointmentService appointmentService,
            IHealthRecordService healthRecordService,
            Core.Patients.Patient patient)
        {
            Patient = patient;
            HealthRecord = healthRecordService.Get(patient);

            _patientHeight = HealthRecord.Height.ToString() + "cm";
            _patientWeight = HealthRecord.Weight.ToString() + "kg";

            foreach (string allergen in HealthRecord.Allergens)
            {
                _patientAllergens += "- " + allergen + "\n";
            }
            foreach (string disease in HealthRecord.PreviousDiseases)
            {
                _patientPreviousDiseases += "- " + disease + "\n";
            }

            _sortCriteria = new List<string>()
            {
                "Date",
                "Doctor",
                "Professional area"
            };
            ChosenSortCriteria = _sortCriteria[0];

            _appointments = new List<AppointmentViewModel>();
            List<Appointment> finishedAppointment = appointmentService.GetPatientFinishedAppointments(HealthRecord.ID);
            foreach (Appointment appointment in finishedAppointment)
            {
                _appointments.Add(new AppointmentViewModel(appointment));
            }

            SortAppointments = new SortAppointmentsCommand(
                this,
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
                        new HospitalRoomRepository()));
            SearchAppointments = new SearchAppointmentsCommand(
                this,
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
                        new HospitalRoomRepository()));
            ShowAnamnesis = new ShowAnamnesisCommand(
                this,
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
                        new HospitalRoomRepository()));
        }
    }
}
