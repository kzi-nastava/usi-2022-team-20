﻿using HealthCareCenter.Core.Appointments.Models;
using HealthCareCenter.Core.HealthRecords;
using HealthCareCenter.Core.Medicine.Models;
using HealthCareCenter.Core.Medicine.Services;
using HealthCareCenter.Core.Notifications.Repositories;
using HealthCareCenter.Core.Patients;
using HealthCareCenter.Core.Prescriptions;
using HealthCareCenter.Core.Rooms.Services;
using HealthCareCenter.Core.Users.Models;
using System;
using System.Collections.Generic;

namespace HealthCareCenter.Core.Notifications.Services
{
    public class NotificationService : INotificationService
    {
        private readonly BaseNotificationRepository _repository;

        public NotificationService(BaseNotificationRepository repository)
        {
            _repository = repository;
        }

        public List<Notification> GetUnopened(User user)
        {
            List<Notification> notifications = new List<Notification>();
            foreach (Notification notification in _repository.Notifications)
            {
                if (notification.UserID == user.ID && !notification.Opened)
                {
                    notification.Opened = true;
                    notifications.Add(notification);
                }
            }
            _repository.Save();
            return notifications;
        }

        public void Send(int userID, string message)
        {
            _repository.CalculateMaxID();
            Notification notification = new Notification(++_repository.maxID, message, userID);
            _repository.Notifications.Add(notification);
            _repository.Save();
        }

        public Dictionary<int, Dictionary<int, int>> GetNotificationsSentDict(List<Prescription> patientPrescriptions)
        {
            // dictionary notificationsSent contains int key and Dictionary<int, int> as value
            // key is prescription id, and value is Dictionary<int, int> where the key is medicine instruction
            // id and value is the index of the time in MedicineInstruction.ConsumptionTime that should be checked
            // if it fulfills the criteria for sending a notification

            Dictionary<int, Dictionary<int, int>> notificationsFromPrescriptionsToSend = new Dictionary<int, Dictionary<int, int>>();
            foreach (Prescription prescription in patientPrescriptions)
            {
                Dictionary<int, int> notificationsToSend = new Dictionary<int, int>();
                foreach (int medicineInstructionID in prescription.MedicineInstructionIDs)
                {
                    MedicineInstruction instruction = MedicineInstructionService.GetSingle(medicineInstructionID);
                    notificationsToSend.Add(instruction.ID, 0);
                    foreach (DateTime takingTime in instruction.ConsumptionTime)
                    {
                        TimeSpan timePassedTakingMedicine = takingTime.TimeOfDay.Subtract(DateTime.Now.TimeOfDay);
                        int hoursTilConsumption = (int)Math.Round(timePassedTakingMedicine.TotalHours);
                        if (hoursTilConsumption < 0)
                        {
                            ++notificationsToSend[instruction.ID];
                            continue;
                        }
                        break;
                    }
                }

                notificationsFromPrescriptionsToSend.Add(prescription.ID, notificationsToSend);
            }

            return notificationsFromPrescriptionsToSend;
        }

        public string Send(Appointment postponedAppointment, Appointment newAppointment, Patient patient)
        {
            string notificationToShowNow = null;
            HealthRecord postponedRecord = HealthRecordService.Get(postponedAppointment);

            _repository.CalculateMaxID();
            Notification postponedPatientNotification = new Notification(++_repository.maxID, $"The appointment you had scheduled at {newAppointment.ScheduledDate} has been postponed to {postponedAppointment.ScheduledDate}.", postponedRecord.PatientID);
            Notification postponedDoctorNotification = new Notification(++_repository.maxID, $"The appointment you had scheduled at {newAppointment.ScheduledDate} has been postponed to {postponedAppointment.ScheduledDate}.", postponedAppointment.DoctorID);
            Notification newDoctorNotification = new Notification(++_repository.maxID, $"A new urgent appointment has been scheduled for you at {newAppointment.ScheduledDate} in room {HospitalRoomService.Get(newAppointment.HospitalRoomID).Name}.", newAppointment.DoctorID);

            if (postponedRecord.PatientID == patient.ID)
            {
                postponedPatientNotification.Opened = true;
                notificationToShowNow = postponedPatientNotification.Message;
            }

            _repository.Notifications.AddRange(new Notification[] { postponedPatientNotification, postponedDoctorNotification, newDoctorNotification });
            _repository.Save();
            return notificationToShowNow;
        }

        public List<string> GetNotifications(Dictionary<int, Dictionary<int, int>> notificationsFromPrescriptionsToSend, Patient patient)
        {
            List<string> notificationsToSend = new List<string>();
            foreach (KeyValuePair<int, Dictionary<int, int>> kvp in notificationsFromPrescriptionsToSend)
            {
                foreach (KeyValuePair<int, int> intructionNotificationTimeIndex in kvp.Value)
                {
                    MedicineInstruction instruction = MedicineInstructionService.GetSingle(intructionNotificationTimeIndex.Key);
                    if (intructionNotificationTimeIndex.Value >= instruction.ConsumptionTime.Count)
                    {
                        continue;
                    }

                    DateTime takingTime = instruction.ConsumptionTime[intructionNotificationTimeIndex.Value];
                    TimeSpan timePassedTakingMedicine = takingTime.TimeOfDay.Subtract(DateTime.Now.TimeOfDay);
                    int hoursTilConsumption = (int)Math.Round(timePassedTakingMedicine.TotalHours);
                    if (hoursTilConsumption > 0 && hoursTilConsumption <= patient.NotificationReceiveTime)
                    {
                        string medicineName = MedicineService.GetName(instruction.MedicineID);
                        string notificationInfo = $"Medicine consumption notification! Medicine: {medicineName}, Time to take: {takingTime.TimeOfDay}, Prescription ID: {kvp.Key}";
                        notificationsToSend.Add(notificationInfo);
                        ++kvp.Value[intructionNotificationTimeIndex.Key];
                    }
                    break;
                }
            }

            return notificationsToSend;
        }
    }
}