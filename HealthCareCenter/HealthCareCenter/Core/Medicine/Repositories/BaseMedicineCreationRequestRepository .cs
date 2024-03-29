﻿using HealthCareCenter.Core.Medicine.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCareCenter.Core.Medicine.Repositories
{
    public abstract class BaseMedicineCreationRequestRepository
    {
        public List<MedicineCreationRequest> Requests;
        private int LargestID { get; set; }

        public abstract int CalculateMaxID();

        public abstract List<MedicineCreationRequest> Load();

        public abstract void Save();
    }
}