﻿using HealthCareCenter.Core.Appointments.Models;
using HealthCareCenter.Core.Appointments.Repository;
using HealthCareCenter.Core.Appointments.Services;
using HealthCareCenter.Core.Notifications.Services;
using HealthCareCenter.Core.Patients;
using HealthCareCenter.Core.Rooms.Models;
using HealthCareCenter.Core.Rooms.Repositories;
using HealthCareCenter.Core.Rooms.Services;
using HealthCareCenter.Core.Users;
using HealthCareCenter.Core.Users.Models;
using HealthCareCenter.Core.Users.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HealthCareCenter.Core.Appointments.Urgent.Services
{
    public class UrgentAppointmentService : BaseUrgentAppointmentService
    {
        private readonly ITermsService _termsService;
        private readonly INotificationService _notificationService;
        private readonly BaseAppointmentRepository _appointmentRepository;
        private readonly IAppointmentService _appointmentService;
        private readonly IHospitalRoomService _hospitalRoomService;
        private readonly BaseHospitalRoomRepository _hospitalRoomRepository;
        private readonly BaseUserRepository _userRepository;
        private readonly IDoctorService _doctorService;

        public UrgentAppointmentService(
            ITermsService service, 
            INotificationService notificationService,
            BaseAppointmentRepository appointmentRepository,
            IAppointmentService appointmentService,
            IHospitalRoomService hospitalRoomService,
            BaseHospitalRoomRepository hospitalRoomRepository,
            BaseUserRepository userRepository,
            IDoctorService doctorService)
        {
            _termsService = service;
            _notificationService = notificationService;
            _appointmentRepository = appointmentRepository;
            _appointmentService = appointmentService;
            _hospitalRoomService = hospitalRoomService;
            _hospitalRoomRepository = hospitalRoomRepository;
            _userRepository = userRepository;
            _doctorService = doctorService;
        }

        public override bool CheckTermAndRemoveUnavailables(DateTime potentialTime, List<Doctor> availableDoctors, List<HospitalRoom> availableRooms, List<Appointment> appointments, Patient patient)
        {
            foreach (Appointment appointment in appointments)
            {
                if (appointment.ScheduledDate.CompareTo(potentialTime) != 0)
                {
                    continue;
                }
                if (appointment.HealthRecordID == patient.HealthRecordID)
                {
                    return false;
                }
                _doctorService.RemoveUnavailableDoctors(availableDoctors, appointment);
                _hospitalRoomService.RemoveUnavailableRooms(availableRooms, appointment);
            }
            return true;
        }

        public override void PrepareForPotentialPostponing(List<Doctor> doctors, List<HospitalRoom> rooms, DateTime potentialTime, Patient patient)
        {
            List<Appointment> appointments = new List<Appointment>(_appointmentRepository.Appointments);
            for (int i = 0; i < _appointmentRepository.Appointments.Count; i++)
            {
                if (_appointmentRepository.Appointments[i].ScheduledDate.CompareTo(potentialTime) != 0)
                    continue;

                appointments.Remove(_appointmentRepository.Appointments[i]);

                List<Doctor> availableDoctors = new List<Doctor>(doctors);
                List<HospitalRoom> availableRooms = new List<HospitalRoom>(rooms);

                bool isValid = CheckTermAndRemoveUnavailables(potentialTime, availableDoctors, availableRooms, appointments, patient);
                if (isValid && availableDoctors.Count > 0 && availableRooms.Count > 0)
                {
                    AddPostponingInfo(availableDoctors, availableRooms, i);
                }

                appointments.Add(_appointmentRepository.Appointments[i]);
            }
        }

        private void AddPostponingInfo(List<Doctor> availableDoctors, List<HospitalRoom> availableRooms, int index)
        {
            UrgentInfo.OccupiedAppointments.Add(_appointmentRepository.Appointments[index]);
            UrgentInfo.NewAppointments.Add(_appointmentRepository.Appointments[index].ID,
                new Appointment { DoctorID = availableDoctors[0].ID, HospitalRoomID = availableRooms[0].ID });
        }

        public override bool GetTermsAndSchedule(List<Doctor> doctors, AppointmentType type, Patient patient)
        {
            List<HospitalRoom> rooms = _hospitalRoomService.GetRoomsOfType(type);
            foreach (string term in _termsService.GetTermsWithinTwoHours())
            {
                DateTime potentialTime = _termsService.CreateTime(term);

                List<Doctor> availableDoctors = new List<Doctor>(doctors);
                List<HospitalRoom> availableRooms = new List<HospitalRoom>(rooms);

                bool isValid = CheckTermAndRemoveUnavailables(potentialTime, availableDoctors, availableRooms, _appointmentRepository.Appointments, patient);

                if (isValid && availableDoctors.Count > 0 && availableRooms.Count > 0)
                {
                    _appointmentService.Schedule(new Appointment(potentialTime, availableRooms[0].ID, availableDoctors[0].ID, patient.HealthRecordID, type, true), false);
                    return true;
                }
                else
                {
                    PrepareForPotentialPostponing(doctors, rooms, potentialTime, patient);
                }
            }
            return false;
        }

        public override bool TryScheduling(AppointmentType type, string doctorType, Patient patient)
        {
            List<Doctor> doctors = _doctorService.GetDoctorsOfType(doctorType);

            UrgentInfo.OccupiedAppointments = new List<Appointment>();
            UrgentInfo.NewAppointments = new Dictionary<int, Appointment>();

            if (!GetTermsAndSchedule(doctors, type, patient))
            {
                return false;
            }
            return true;
        }

        public override Appointment Postpone(ref string notification, Patient patient, AppointmentType type, OccupiedAppointment selectedAppointment)
        {
            Appointment postponedAppointment = _appointmentService.Get(selectedAppointment);

            Appointment newAppointment = new Appointment(selectedAppointment.ScheduledDate, OccupiedInfo.NewAppointments[postponedAppointment.ID].HospitalRoomID, OccupiedInfo.NewAppointments[postponedAppointment.ID].DoctorID, patient.HealthRecordID, type, true);
            _appointmentRepository.Appointments.Add(newAppointment);
            postponedAppointment.ScheduledDate = selectedAppointment.PostponedTime;
            _appointmentRepository.Save();

            _hospitalRoomService.Update(newAppointment.HospitalRoomID, newAppointment);
            _hospitalRoomRepository.Save();

            notification = _notificationService.Send(postponedAppointment, newAppointment, patient);
            return postponedAppointment;
        }

        public override List<OccupiedAppointment> GetAppointmentsForDisplay()
        {
            List<OccupiedAppointment> appointments = new List<OccupiedAppointment>();
            foreach (Appointment appointment in OccupiedInfo.OccupiedAppointments)
            {
                OccupiedAppointment appointmentDisplay = new OccupiedAppointment(appointment.ID, appointment.Type, appointment.ScheduledDate, appointment.Emergency, OccupiedInfo.AppointmentPostponableTo[appointment.ID]);
                LinkDoctor(appointment, appointmentDisplay);
                LinkPatient(appointment, appointmentDisplay);
                appointments.Add(appointmentDisplay);
            }
            return appointments;
        }

        private void LinkPatient(Appointment appointment, OccupiedAppointment appointmentDisplay)
        {
            foreach (Patient patient in _userRepository.Patients)
            {
                if (appointment.HealthRecordID == patient.HealthRecordID)
                {
                    appointmentDisplay.PatientName = patient.FirstName + " " + patient.LastName;
                    return;
                }
            }
        }

        private void LinkDoctor(Appointment appointment, OccupiedAppointment appointmentDisplay)
        {
            foreach (Doctor doctor in _userRepository.Doctors)
            {
                if (appointment.DoctorID == doctor.ID)
                {
                    appointmentDisplay.DoctorName = doctor.FirstName + " " + doctor.LastName;
                    return;
                }
            }
        }

        public override bool IsPostponableTo(DateTime newTime, Appointment occupiedAppointment)
        {
            foreach (Appointment appointment in _appointmentRepository.Appointments)
            {
                if (appointment.ScheduledDate.CompareTo(newTime) != 0)
                {
                    continue;
                }

                if (appointment.DoctorID == occupiedAppointment.DoctorID || appointment.HospitalRoomID == occupiedAppointment.HospitalRoomID || appointment.HealthRecordID == occupiedAppointment.HealthRecordID)
                {
                    return false;
                }
            }
            return true;
        }

        public override void SortPostponableAppointments()
        {
            List<string> allPossibleTerms = _termsService.GetPossibleDailyTerms();
            List<string> terms = _termsService.GetTermsAfterTwoHours(allPossibleTerms);
            List<Appointment> sortedAppointments = new List<Appointment>();
            Dictionary<int, DateTime> newDateOf = new Dictionary<int, DateTime>();
            bool foundAll = false;
            DateTime current = DateTime.Now;

            for (int i = 0; i < 365; i++)
            {
                foreach (string term in terms)
                {
                    int hours = int.Parse(term.Split(":")[0]);
                    int minutes = int.Parse(term.Split(":")[1]);
                    DateTime newTime = current.Date.AddHours(hours).AddMinutes(minutes);

                    foreach (Appointment occupiedAppointment in OccupiedInfo.OccupiedAppointments.ToList())
                    {
                        if (!IsPostponableTo(newTime, occupiedAppointment))
                            continue;

                        sortedAppointments.Add(occupiedAppointment);
                        newDateOf.Add(occupiedAppointment.ID, newTime);
                        OccupiedInfo.OccupiedAppointments.Remove(occupiedAppointment);

                        if (sortedAppointments.Count == 5)
                        {
                            foundAll = true;
                            break;
                        }
                    }
                    if (foundAll)
                        break;
                }
                if (foundAll)
                    break;

                current = current.AddDays(1);
                terms = new List<string>(allPossibleTerms);
            }
            OccupiedInfo.OccupiedAppointments = new List<Appointment>(sortedAppointments);
            OccupiedInfo.AppointmentPostponableTo = new Dictionary<int, DateTime>(newDateOf);
        }
    }
}
