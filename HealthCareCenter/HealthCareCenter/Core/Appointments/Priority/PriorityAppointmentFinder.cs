﻿using HealthCareCenter.Core.Appointments.Models;
using HealthCareCenter.Core.Appointments.Repository;
using HealthCareCenter.Core.Rooms.Services;
using HealthCareCenter.Core.Users;
using HealthCareCenter.Core.Users.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCareCenter.Core.Appointments.Services.Priority
{
    class PriorityAppointmentFinder : IPriorityAppointmentFinder
    {
        // make attributes for IHospitalRoomService and BaseAppointmentRepository
        private readonly BaseAppointmentRepository _appointmentRepository;
        private readonly IAppointmentTermService _termService;
        private readonly BaseUserRepository _userRepository;
        private readonly IHospitalRoomService _hospitalRoomService;

        public PriorityAppointmentFinder(
            IAppointmentTermService termService,
            BaseAppointmentRepository appointmentRepository,
            BaseUserRepository userRepository,
            IHospitalRoomService hospitalRoomService)
        {
            _termService = termService;
            _appointmentRepository = appointmentRepository;
            _userRepository = userRepository;
            _hospitalRoomService = hospitalRoomService;
        }

        public Appointment BothPrioritiesSearch(int doctorID, int healthRecordID, DateTime finalScheduleDate, AppointmentTerm startRange, AppointmentTerm endRange)
        {
            List<AppointmentTerm> possibleSchedules = _termService.GetDailyTermsFromRange(startRange, endRange);

            return PrioritySearch(doctorID, finalScheduleDate, healthRecordID, possibleSchedules);
        }

        public Appointment DifferentDoctorSameTimeSearch(int doctorID, int healthRecordID, DateTime finalScheduleDate, AppointmentTerm startRange, AppointmentTerm endRange)
        {
            List<AppointmentTerm> possibleTerms = _termService.GetDailyTermsFromRange(startRange, endRange);
            foreach (Doctor doctor in _userRepository.Doctors)
            {
                if (doctor.ID == doctorID)
                {
                    continue;
                }

                Appointment newAppointment = PrioritySearch(doctor.ID, finalScheduleDate, healthRecordID, possibleTerms);
                if (newAppointment != null)
                {
                    return newAppointment;
                }
            }

            return null;
        }

        public Appointment GetDoctorPriorityAppointment(int doctorID, int healthRecordID, DateTime finalScheduleDate, AppointmentTerm startRange, AppointmentTerm endRange)
        {
            Appointment newAppointment = BothPrioritiesSearch(doctorID, healthRecordID, finalScheduleDate, startRange, endRange);

            if (newAppointment == null)
            {
                newAppointment = SameDoctorDifferentTimeSearch(doctorID, healthRecordID, finalScheduleDate, startRange, endRange);
            }

            return newAppointment;
        }

        public Appointment GetTimePriorityAppointment(int doctorID, int healthRecordID, DateTime finalScheduleDate, AppointmentTerm startRange, AppointmentTerm endRange)
        {
            Appointment newAppointment = BothPrioritiesSearch(doctorID, healthRecordID, finalScheduleDate, startRange, endRange);

            if (newAppointment == null)
            {
                newAppointment = DifferentDoctorSameTimeSearch(doctorID, healthRecordID, finalScheduleDate, startRange, endRange);
            }

            return newAppointment;
        }

        public Appointment PrioritySearch(int doctorID, DateTime finalScheduleDate, int healthRecordID, List<AppointmentTerm> possibleTerms)
        {
            DateTime date = DateTime.Now;
            date = date.AddDays(1);
            while (date.Date.CompareTo(finalScheduleDate.Date) <= 0)
            {
                foreach (AppointmentTerm term in possibleTerms)
                {
                    bool isAvailable = true;
                    foreach (Appointment appointment in _appointmentRepository.Appointments)
                    {
                        if (doctorID == appointment.DoctorID &&
                            term.Hours == appointment.ScheduledDate.Hour &&
                            term.Minutes == appointment.ScheduledDate.Minute &&
                            appointment.ScheduledDate.Date.CompareTo(date.Date) == 0)
                        {
                            isAvailable = false;
                            break;
                        }
                    }

                    if (isAvailable)
                    {
                        string scheduleDateParse = date.ToString().Split(" ")[0] + " " + term.ToString();
                        DateTime scheduleDate = Convert.ToDateTime(scheduleDateParse);

                        int hospitalRoomID = _hospitalRoomService.GetAvailableRoomID(scheduleDate, RoomType.Checkup);
                        if (hospitalRoomID == -1)
                        {
                            continue;
                        }

                        Appointment newAppointment = new Appointment()
                        {
                            ID = _appointmentRepository.GetLargestID() + 1,
                            Type = AppointmentType.Checkup,
                            ScheduledDate = scheduleDate,
                            CreatedDate = DateTime.Now,
                            Emergency = false,
                            DoctorID = doctorID,
                            HealthRecordID = healthRecordID,
                            HospitalRoomID = hospitalRoomID,
                        };

                        return newAppointment;
                    }
                }
                date = date.AddDays(1);
            }

            return null;
        }

        public Appointment SameDoctorDifferentTimeSearch(int doctorID, int healthRecordID, DateTime finalScheduleDate, AppointmentTerm startRange, AppointmentTerm endRange)
        {
            List<AppointmentTerm> possibleTerms = _termService.GetDailyTermsOppositeOfRange(startRange, endRange);

            return PrioritySearch(doctorID, finalScheduleDate, healthRecordID, possibleTerms);
        }
    }
}
