﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCareCenter.Core.Users.Models
{
    public class Manager : User
    {
        public List<int> RenovationScheduleIDs { get; set; }
        public List<int> MedicineSuggestionIDs { get; set; }
        //public List<Survey> Surveys { get; set; }
    }
}