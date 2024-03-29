﻿using System;
using System.Collections.Generic;
using System.Text;
using HealthCareCenter.Core.HealthRecords;
using HealthCareCenter.Core.Patients.Services;
using HealthCareCenter.Core.Users;
using HealthCareCenter.Core.Users.Models;

namespace HealthCareCenter.Core.Patients.Controllers
{
    public class PatientEditController : PatientController
    {
        private readonly IPatientService _patientService;
        private readonly BaseUserRepository _userRepository;

        public PatientEditController(IPatientService patientService, BaseUserRepository userRepository)
        {
            _patientService = patientService;
            _userRepository = userRepository;
        }

        public void Edit(PatientDTO editedPatientDTO, HealthRecordDTO editedRecordDTO, Patient uneditedPatient, HealthRecord uneditedRecord)
        {
            try
            {
                CheckIfDataEntered(editedPatientDTO, editedRecordDTO);
                ValidateData(editedPatientDTO, editedRecordDTO, uneditedPatient);
            }
            catch (Exception)
            {
                throw;
            }

            double height = double.Parse(editedRecordDTO.Height);
            double weight = double.Parse(editedRecordDTO.Weight);

            HealthRecord editedRecord = new HealthRecord(editedRecordDTO.ID, height, weight, editedRecordDTO.PreviousDiseases, editedRecordDTO.Allergens, editedRecordDTO.PatientID);
            Patient editedPatient = new Patient(editedPatientDTO.ID, editedPatientDTO.Username, editedPatientDTO.Password, editedPatientDTO.FirstName, editedPatientDTO.LastName, (DateTime)editedPatientDTO.DateOfBirth, editedPatientDTO.IsBlocked, editedPatientDTO.BlockedBy, editedPatientDTO.PrescriptionIDs, editedPatientDTO.HealthRecordID);

            _patientService.Edit(uneditedPatient, uneditedRecord, editedPatient, editedRecord);
        }

        private void ValidateData(PatientDTO patient, HealthRecordDTO record, Patient uneditedPatient)
        {
            try
            {
                ValidateBirthDate(patient);
                ValidateUsername(patient, uneditedPatient);
                ValidateHeight(record);
                ValidateWeight(record);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ValidateUsername(PatientDTO editedPatient, Patient uneditedPatient)
        {
            if (uneditedPatient.Username == editedPatient.Username)
            {
                return;
            }
            foreach (User user in _userRepository.Users)
            {
                if (user.Username == editedPatient.Username)
                {
                    throw new Exception("Username is already in use. Choose a different one.");
                }
            }
        }
    }
}
