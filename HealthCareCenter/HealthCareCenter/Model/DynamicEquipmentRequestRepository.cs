﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HealthCareCenter.Model
{
    public static class DynamicEquipmentRequestRepository
    {
        public static List<DynamicEquipmentRequest> Requests { get; set; }
        public static int maxID = -1;

        public static void CalculateMaxID()
        {
            maxID = -1;
            foreach (DynamicEquipmentRequest request in DynamicEquipmentRequestRepository.Requests)
            {
                if (request.ID > maxID)
                {
                    maxID = request.ID;
                }
            }
        }

        public static void Load()
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    DateFormatString = Constants.DateTimeFormat
                };

                string JSONTextRequests = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\data\dynamicEquipmentRequests.json");
                Requests = (List<DynamicEquipmentRequest>)JsonConvert.DeserializeObject<IEnumerable<DynamicEquipmentRequest>>(JSONTextRequests, settings);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Save()
        {
            try
            {
                JsonSerializer serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented,
                    DateFormatString = Constants.DateTimeFormat
                };

                using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\data\dynamicEquipmentRequests.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, Requests);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}