﻿using System.Collections.Generic;
using System.Linq;
using HealthCareCenter.Core.Rooms.Repositories;

namespace HealthCareCenter.Core.Rooms.Models
{
    public class HospitalRoom : Room
    {
        public RoomType Type { get; set; }

        public string Name { get; set; }

        public List<int> AppointmentIDs { get; set; }

        public HospitalRoom()
        { }

        /// <summary>
        /// Constructor used for adding new hospital room.
        /// </summary>
        public HospitalRoom(RoomType type, string name)
        {
            RoomRepository roomRepository = new RoomRepository(
                new HospitalRoomUnderConstructionRepository(),
                new HospitalRoomForRenovationRepository(),
                new HospitalRoomRepository());
            List<int> largestIDs = roomRepository.GetLargestIDs();

            ID = largestIDs.Max() + 1;
            Name = name;
            EquipmentAmounts = new Dictionary<string, int>();
            EquipmentRearrangementsIDs = new List<int>();
            Type = type;
            AppointmentIDs = new List<int>();
        }
    }
}