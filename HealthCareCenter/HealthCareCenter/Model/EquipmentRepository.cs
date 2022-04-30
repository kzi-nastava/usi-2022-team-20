﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HealthCareCenter.Model
{
    internal class EquipmentRepository
    {
        public static List<Equipment> Equipments = LoadEquipments();

        /// <summary>
        /// Load all equipments from file equipments.json.
        /// </summary>
        /// <returns>List of all hospital rooms.</returns>
        private static List<Equipment> LoadEquipments()
        {
            try
            {
                List<Equipment> equipments = new List<Equipment>();
                var settings = new JsonSerializerSettings
                {
                    DateFormatString = Constants.DateFormat
                };

                string JSONTextEquipments = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\data\equipments.json");
                equipments = (List<Equipment>)JsonConvert.DeserializeObject<IEnumerable<Equipment>>(JSONTextEquipments, settings);
                return equipments;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Replace all data from file equipments.json with list equipments.
        /// </summary>
        /// <param name="equipments">Data that will replace the old ones.</param>
        /// <returns>True if data update performed properly.</returns>
        public static bool SaveEquipments(List<Equipment> equipments)
        {
            try
            {
                JsonSerializer serializer = new JsonSerializer();

                using (StreamWriter sw = new StreamWriter(@"..\..\..\data\equipments.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, equipments);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}