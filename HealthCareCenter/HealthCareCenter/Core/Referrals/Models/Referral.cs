﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCareCenter.Core.Referrals.Models
{
    public class Referral
    {
        public int ID { get; set; }
        public int PatientID { get; set; }
        public int DoctorID { get; set; }
    }
}