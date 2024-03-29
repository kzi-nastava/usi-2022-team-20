﻿using HealthCareCenter.Core.Appointments.Models;
using HealthCareCenter.Core.Appointments.Repository;
using HealthCareCenter.Core.Patients;
using HealthCareCenter.Core.Referrals.Models;
using HealthCareCenter.Core.Referrals.Repositories;
using HealthCareCenter.Core.Rooms.Repositories;
using HealthCareCenter.Core.Rooms.Services;
using HealthCareCenter.Core.Users;
using HealthCareCenter.Core.Users.Models;
using System.Collections.Generic;

namespace HealthCareCenter.Core.Referrals.Services
{
    public class ReferralService : IReferralService
    {
        private readonly BaseReferralRepository _referralsRepository;
        private readonly BaseAppointmentRepository _appointmentRepository;
        private readonly IHospitalRoomService _hospitalRoomService;
        private readonly BaseUserRepository _userRepository;
        private readonly BaseHospitalRoomRepository _hospitalRoomRepository;


        public ReferralService(
            BaseReferralRepository referralsRepository,
            BaseAppointmentRepository appointmentRepository,
            IHospitalRoomService hospitalRoomService,
            BaseUserRepository userRepository,
            BaseHospitalRoomRepository hospitalRoomRepository)
        {
            _referralsRepository = referralsRepository;
            _appointmentRepository = appointmentRepository;
            _hospitalRoomService = hospitalRoomService;
            _userRepository = userRepository;
            _hospitalRoomRepository = hospitalRoomRepository;
        }

        public List<PatientReferralForDisplay> Get(Patient patient)
        {
            List<PatientReferralForDisplay> referrals = new List<PatientReferralForDisplay>();
            foreach (Referral referral in _referralsRepository.Referrals)
            {
                if (referral.PatientID != patient.ID)
                {
                    continue;
                }

                Add(referral, referrals);
            }
            return referrals;
        }

        private void Add(Referral referral, List<PatientReferralForDisplay> referrals)
        {
            PatientReferralForDisplay patientReferral = new PatientReferralForDisplay(referral.ID);

            LinkDoctor(referral, patientReferral);
            referrals.Add(patientReferral);
        }

        private void LinkDoctor(Referral referral, PatientReferralForDisplay patientReferral)
        {
            foreach (Doctor doctor in _userRepository.Doctors)
            {
                if (doctor.ID != referral.DoctorID)
                {
                    continue;
                }
                patientReferral.DoctorUsername = doctor.Username;
                patientReferral.DoctorFirstName = doctor.FirstName;
                patientReferral.DoctorLastName = doctor.LastName;
                return;
            }
        }

        public Referral Get(int referralID)
        {
            foreach (Referral referral in _referralsRepository.Referrals)
            {
                if (referral.ID == referralID)
                {
                    return referral;
                }
            }
            return null;
        }

        public void Schedule(Referral referral, Appointment appointment)
        {
            _appointmentRepository.Appointments.Add(appointment);
            _appointmentRepository.Save();

            _hospitalRoomService.Update(appointment.HospitalRoomID, appointment);
            _hospitalRoomRepository.Save();

            _referralsRepository.Referrals.Remove(referral);
            _referralsRepository.Save();
        }

        public void Fill(int doctorID, int patientID, Referral referral)
        {
            referral.ID = _referralsRepository.LargestID;
            referral.DoctorID = doctorID;
            referral.PatientID = patientID;
        }
    }
}
