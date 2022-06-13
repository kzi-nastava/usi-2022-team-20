﻿using HealthCareCenter.Core.Equipment.Services;
using HealthCareCenter.Core.Rooms.Models;
using HealthCareCenter.Core.Rooms.Services;
using System;

namespace HealthCareCenter.Core.Equipment.Controllers
{
    public class DistributeDynamicEquipmentController
    {
        private IDynamicEquipmentService _dynamicEquipmentService;

        public DistributeDynamicEquipmentController(IDynamicEquipmentService dynamicEquipmentService)
        {
            _dynamicEquipmentService = dynamicEquipmentService;
        }

        public void Transfer(string quantityString, string[] equipmentAndAmount, Room roomWithEquipment, Room roomWithShortage, Room storage)
        {
            if (roomWithEquipment.ID == roomWithShortage.ID)
            {
                throw new Exception("You must select different rooms.");
            }

            if (!int.TryParse(quantityString, out int quantity) || quantity <= 0)
            {
                throw new Exception("Quantity must be a positive number.");
            }

            string selectedEquipment = equipmentAndAmount[0];
            int selectedEquipmentAmount = int.Parse(equipmentAndAmount[1]);

            if (quantity > selectedEquipmentAmount)
            {
                throw new Exception("You cannot enter a quantity bigger than the amount of equipment available in the room.");
            }

            _dynamicEquipmentService.Transfer(quantity, selectedEquipment, roomWithEquipment, roomWithShortage, storage);
        }

        public Room GetRoom(int roomID)
        {
            return HospitalRoomService.Get(roomID);
        }
    }
}