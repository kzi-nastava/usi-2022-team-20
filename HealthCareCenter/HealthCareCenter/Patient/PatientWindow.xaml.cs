﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HealthCareCenter.Model;
using System.Data;

namespace HealthCareCenter
{
    public partial class PatientWindow : Window
    {
        private Patient signedUser;

        // appointment datagrid
        private DataTable appointmentsDataTable;

        // scheduling appointment items
        private DataTable allDoctorsDataTable;
        private DataTable allAvailableTimeTable;
        private DataRowView chosenDoctor;  // selected row with doctor information in allDoctorsDataGrid
        private DateTime chosenScheduleDate;
        private bool isScheduleDateChosen = false;
        private DataRowView chosenAppointment;  // selected row with appointment information, used when making changes to an appointment
        private bool shouldSendRequestToSecretary = false;

        // trolling limits
        private int creationTrollLimit = 8;
        private int modificationTrollLimit = 5;

        public PatientWindow(User user)
        {
            signedUser = (Patient)user;
            if (signedUser.IsBlocked)
            {
                MessageBox.Show("This user is blocked\nReturning to login screen");
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
            PatientDataManager.Load(signedUser);
            InitializeComponent();

            // creating the appointment table and making that window visible
            ClearWindow();
            myAppointmentsGrid.Visibility = Visibility.Visible;
            CreateAppointmentTable();
            FillAppointmentTable();
            currentActionTextBlock.Text = "My appointments";

        }

        private void ClearWindow()
        {
            myAppointmentsGrid.Visibility = Visibility.Collapsed;
            createAppointmentGrid.Visibility = Visibility.Collapsed;
            if (allAvailableTimeTable != null)
            {
                allAvailableTimeTable.Clear();
            }    
            chosenDoctor = null;
            isScheduleDateChosen = false;
            dayChoiceComboBox.Items.Clear();
            monthChoiceComboBox.Items.Clear();
            yearChoiceComboBox.Items.Clear();
            shouldSendRequestToSecretary = false;

            searchDoctorsGrid.Visibility = Visibility.Collapsed;

            myNotificationGrid.Visibility = Visibility.Collapsed;

            myHealthRecordGrid.Visibility = Visibility.Collapsed;

            doctorSurveyGrid.Visibility = Visibility.Collapsed;

            healthCenterGrid.Visibility = Visibility.Collapsed;

        }

        // creation of appointment table
        //==============================================================================
        private void CreateAppointmentTable()
        {
            appointmentsDataTable = new DataTable("Appointments");
            appointmentsDataTable.Columns.Add(new DataColumn("Id", typeof(int)));
            appointmentsDataTable.Columns.Add(new DataColumn("Type of appointment", typeof(string)));
            appointmentsDataTable.Columns.Add(new DataColumn("Appointment date", typeof(string)));
            appointmentsDataTable.Columns.Add(new DataColumn("Creation date", typeof(string)));
            appointmentsDataTable.Columns.Add(new DataColumn("Emergency", typeof(bool)));
            appointmentsDataTable.Columns.Add(new DataColumn("Doctor's full name", typeof(string)));
            appointmentsDataTable.Columns.Add(new DataColumn("Room", typeof(string)));

        }

        private void FillAppointmentTable()
        {
            DataRow row;
            foreach (Appointment appointment in PatientDataManager.UnfinishedAppointments)
            {
                row = appointmentsDataTable.NewRow();
                row[0] = appointment.ID;
                row[1] = appointment.Type;
                row[2] = appointment.AppointmentDate;
                row[3] = appointment.CreatedDate;
                row[4] = appointment.Emergency;
                row[5] = PatientDataManager.GetDoctorFullNameFromAppointment(appointment);
                row[6] = appointment.HospitalRoomID;  // replace with hospital room type
                appointmentsDataTable.Rows.Add(row);
            }
            myAppointmentsDataGrid.ItemsSource = appointmentsDataTable.DefaultView;
        }
        //==============================================================================

        // creation of available appointments tables
        //==============================================================================

        // doctors table
        //==================================================
        private void CreateDoctorTable()
        {
            allDoctorsDataTable = new DataTable("Doctors");
            allDoctorsDataTable.Columns.Add(new DataColumn("Id", typeof(int)));
            allDoctorsDataTable.Columns.Add(new DataColumn("First name", typeof(string)));
            allDoctorsDataTable.Columns.Add(new DataColumn("Last name", typeof(string)));
            allDoctorsDataTable.Columns.Add(new DataColumn("Type", typeof(string)));

        }

        private void FillDoctorTable()
        {
            DataRow row;
            foreach (Doctor doctor in PatientDataManager.AllDoctors)
            {
                row = allDoctorsDataTable.NewRow();
                row[0] = doctor.ID;
                row[1] = doctor.FirstName;
                row[2] = doctor.LastName;
                row[3] = doctor.Type;
                allDoctorsDataTable.Rows.Add(row);
            }
            allDoctorsDataGrid.ItemsSource = allDoctorsDataTable.DefaultView;
        }
        //==================================================

        // time table
        //==================================================
        private void FillDateComboBox()
        {
            for (int i = 1; i <= 31; i++)
            {
                string s = i.ToString();
                if (s.Length == 1)
                {
                    s = "0" + s;
                }
                dayChoiceComboBox.Items.Add(s);
            }
            for (int i = 1; i <= 12; i++)
            {
                string s = i.ToString();
                if (s.Length == 1)
                {
                    s = "0" + s;
                }
                monthChoiceComboBox.Items.Add(s);
            }
            yearChoiceComboBox.Items.Add("2022");
            yearChoiceComboBox.Items.Add("2023");
            yearChoiceComboBox.Items.Add("2024");
            yearChoiceComboBox.Items.Add("2025");
            yearChoiceComboBox.Items.Add("2026");
        }

        private void CreateAvailableTimeTable()
        {
            allAvailableTimeTable = new DataTable("Available time");
            allAvailableTimeTable.Columns.Add(new DataColumn("Day", typeof(int)));
            allAvailableTimeTable.Columns.Add(new DataColumn("Month", typeof(int)));
            allAvailableTimeTable.Columns.Add(new DataColumn("Year", typeof(int)));
            allAvailableTimeTable.Columns.Add(new DataColumn("Hour", typeof(int)));
            allAvailableTimeTable.Columns.Add(new DataColumn("Minutes", typeof(int)));
        }

        private List<string> GetAllPossibleDailySchedules()
        {
            // returns all schedules from 8:00 to 21:00 knowing that an appointment lasts 15 minutes

            int hours = 8;
            int minutes = 0;
            List<string> allPossibleSchedules = new List<string>();
            while (hours < 21 || minutes < 15)
            {
                string schedule = hours + ":" + minutes;
                allPossibleSchedules.Add(schedule);
                minutes += 15;
                if (minutes >= 60)
                {
                    ++hours;
                    minutes = 0;
                }
            }

            return allPossibleSchedules;
        }

        private void FillAvailableTimeTable()
        {
            List<string> allPossibleSchedules = GetAllPossibleDailySchedules();
            foreach (Appointment appointment in PatientDataManager.AllAppointments)
            {
                if (appointment.DoctorID == Convert.ToInt32(chosenDoctor[0]))
                {
                    if (appointment.AppointmentDate.Date.CompareTo(chosenScheduleDate.Date) == 0)
                    {
                        allPossibleSchedules.Remove(appointment.AppointmentDate.Hour + ":" + appointment.AppointmentDate.Minute);
                    }
                }
            }

            DataRow row;
            foreach (string availableTime in allPossibleSchedules)
            {
                string[] time = availableTime.Split(":");
                row = allAvailableTimeTable.NewRow();
                row[0] = chosenScheduleDate.Day;
                row[1] = chosenScheduleDate.Month;
                row[2] = chosenScheduleDate.Year;
                row[3] = Convert.ToInt32(time[0]);
                row[4] = Convert.ToInt32(time[1]);
                allAvailableTimeTable.Rows.Add(row);
            }
            allAvailableTimeDataGrid.ItemsSource = allAvailableTimeTable.DefaultView;
        }
        //==================================================

        //==============================================================================

        // action menu click methods
        //=======================================================================================
        private void myAppointmentsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClearWindow();
            myAppointmentsGrid.Visibility = Visibility.Visible;
            currentActionTextBlock.Text = "My appointments";
        }

        private void searchDoctorsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClearWindow();
            searchDoctorsGrid.Visibility = Visibility.Visible;
            currentActionTextBlock.Text = "Search for doctor";
        }

        private void myNotificationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClearWindow();
            myNotificationGrid.Visibility = Visibility.Visible;
            currentActionTextBlock.Text = "My notifications";
        }

        private void myHealthRecordMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClearWindow();
            myHealthRecordGrid.Visibility = Visibility.Visible;
            currentActionTextBlock.Text = "My health record";
        }

        private void doctorSurveyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClearWindow();
            doctorSurveyGrid.Visibility = Visibility.Visible;
            currentActionTextBlock.Text = "Doctor survey";
        }

        private void healthCenterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClearWindow();
            healthCenterGrid.Visibility = Visibility.Visible;
            currentActionTextBlock.Text = "Health center survey";
        }

        private void logOutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PatientDataManager.WriteAll();

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
        //=======================================================================================

        // my appointment menu button click methods
        //=======================================================================================
        private void createAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            myAppointmentsGrid.Visibility = Visibility.Collapsed;
            createAppointmentGrid.Visibility = Visibility.Visible;
            CreateDoctorTable();
            FillDoctorTable();
            FillDateComboBox();
            chosenAppointment = null;
        }

        private void makeChangeAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            chosenAppointment = myAppointmentsDataGrid.SelectedItem as DataRowView;
            if (chosenAppointment == null)
            {
                MessageBox.Show("No appointment selected");
                return;
            }

            DateTime chosenAppointmentDate = Convert.ToDateTime(chosenAppointment[2]);
            TimeSpan timeTillAppointment = chosenAppointmentDate.Subtract(DateTime.Now);
            if (timeTillAppointment.TotalDays <= 2)
            {
                MessageBox.Show("Since there are less than 2 days left until the appointment starts" +
                    "\nrequest will be sent to the secretary to confirm");
                shouldSendRequestToSecretary = true;
            }
            myAppointmentsGrid.Visibility = Visibility.Collapsed;
            createAppointmentGrid.Visibility = Visibility.Visible;
            CreateDoctorTable();
            FillDoctorTable();
            FillDateComboBox();

        }

        private void cancelAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            chosenAppointment = myAppointmentsDataGrid.SelectedItem as DataRowView;
            if (chosenAppointment == null)
            {
                MessageBox.Show("No appointment selected");
                return;
            }

            DateTime chosenAppointmentDate = Convert.ToDateTime(chosenAppointment[2]);
            TimeSpan timeTillAppointment = chosenAppointmentDate.Subtract(DateTime.Now);
            if (timeTillAppointment.TotalDays <= 2)
            {
                MessageBox.Show("Since there are less than 2 days left until the appointment starts," +
                    "\nrequest will be sent to the secretary to confirm");
                shouldSendRequestToSecretary = true;
            }

            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Cancel appointment", System.Windows.MessageBoxButton.YesNo);
            Appointment forDeletion = new Appointment();
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                CheckModificationTroll();

                if (shouldSendRequestToSecretary)
                {
                    AppointmentChangeRequest changeRequest = new AppointmentChangeRequest();
                    changeRequest.AppointmentID = Convert.ToInt32(chosenAppointment[0]);
                    changeRequest.RequestType = Enums.RequestType.Delete;
                    changeRequest.DateSent = DateTime.Now;
                    changeRequest.PatientID = signedUser.ID;
                    PatientDataManager.AllChangeRequests.Add(changeRequest);
                }
                else
                {
                    foreach (Appointment appointment in PatientDataManager.UnfinishedAppointments)
                    {
                        if (Convert.ToInt32(chosenAppointment[0]) == appointment.ID)
                        {
                            forDeletion = appointment;
                            break;
                        }
                    }
                }
            }
            PatientDataManager.UnfinishedAppointments.Remove(forDeletion);
            CreateAppointmentTable();
            FillAppointmentTable();
        }
        //=======================================================================================

        // appointments creation/modification menu button click methods
        //=======================================================================================
        private void chooseDoctorButton_Click(object sender, RoutedEventArgs e)
        {
            chosenDoctor = allDoctorsDataGrid.SelectedItem as DataRowView;
            if (chosenDoctor == null)
            {
                MessageBox.Show("No doctor selected");
                return;
            }

            var dayChosen = dayChoiceComboBox.SelectedItem;
            var monthChosen = monthChoiceComboBox.SelectedItem;
            var yearChosen = yearChoiceComboBox.SelectedItem;

            if (dayChosen == null || monthChosen == null || yearChosen == null)
            {
                MessageBox.Show("Date combo boxes not filled");
                return;
            }

            string chosenDate = Convert.ToInt32(dayChosen) + "/" + Convert.ToInt32(monthChosen) + "/" + Convert.ToInt32(yearChosen);
            try
            {
                chosenScheduleDate = Convert.ToDateTime(chosenDate);
                isScheduleDateChosen = true;
            }
            catch (Exception formatEx)
            {
                MessageBox.Show("Invalid date format");
                return;
            }

            if (chosenScheduleDate.CompareTo(DateTime.Now) <= 0)
            {
                MessageBox.Show("Invalid date");
                return;
            }


            CreateAvailableTimeTable();
            FillAvailableTimeTable();

        }

        private void scheduleAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (chosenDoctor == null)
            {
                MessageBox.Show("No doctor selected");
                return;
            }
            if (!isScheduleDateChosen)
            {
                MessageBox.Show("No date selected");
                return;
            }

            DataRowView chosenScheduleTime = allAvailableTimeDataGrid.SelectedItem as DataRowView;
            if (chosenScheduleTime == null)
            {
                MessageBox.Show("No schedule time selected");
                return;
            }

            string newAppointmentDateParseString = chosenScheduleTime[0].ToString() + "/" + chosenScheduleTime[1].ToString() + "/" + chosenScheduleTime[2].ToString() + " " + chosenScheduleTime[3].ToString() + ":" + chosenScheduleTime[0].ToString();

            string confirmationMessage;
            if (IsModification())
            {
                confirmationMessage = "Make changes";
            }
            else
            {
                confirmationMessage = "Schedule appointment";
            }
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", confirmationMessage, System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Appointment newAppointment = new Appointment();
                if (IsModification())
                {
                    CheckModificationTroll();
                    if (shouldSendRequestToSecretary)
                    {
                        AppointmentChangeRequest changeRequest = new AppointmentChangeRequest();
                        changeRequest.ID = PatientDataManager.GenerateAppointmentChangeRequestID();
                        changeRequest.AppointmentID = Convert.ToInt32(chosenAppointment[0]);
                        changeRequest.RequestType = Enums.RequestType.Delete;
                        changeRequest.NewDate = Convert.ToDateTime(chosenAppointment[2]);
                        changeRequest.NewAppointmentType = Enums.AppointmentType.Checkup;
                        changeRequest.NewDoctorID = Convert.ToInt32(chosenDoctor[0]);
                        changeRequest.DateSent = DateTime.Now;
                        changeRequest.PatientID = signedUser.ID;
                        PatientDataManager.AllChangeRequests.Add(changeRequest);
                    }
                    else
                    {
                        foreach (Appointment appointment in PatientDataManager.UnfinishedAppointments)
                        {
                            if (Convert.ToInt32(chosenAppointment[0]) == appointment.ID)
                            {
                                newAppointment = appointment;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    CheckCreationTroll();
                    newAppointment.ID = PatientDataManager.GenerateAppointmentID();
                    PatientDataManager.UnfinishedAppointments.Add(newAppointment);
                }

                newAppointment.Type = Enums.AppointmentType.Checkup;
                newAppointment.CreatedDate = DateTime.Now;
                newAppointment.AppointmentDate = Convert.ToDateTime(newAppointmentDateParseString);
                newAppointment.Emergency = false;
                newAppointment.DoctorID = Convert.ToInt32(chosenDoctor[0]);
                newAppointment.HealthRecordID = signedUser.HealthRecordID;
                newAppointment.HospitalRoomID = -1;
                newAppointment.PatientAnamnesis = null;

                ClearWindow();
                CreateAppointmentTable();
                FillAppointmentTable();
                createAppointmentGrid.Visibility = Visibility.Collapsed;
                myAppointmentsGrid.Visibility = Visibility.Visible;
            }

        }

        private bool IsModification()
        {
            // returns true if the user is in the process of modifying the appointment
            // and false if he is in the process of creating a new appointment

            return chosenAppointment != null;
        }
        //=======================================================================================

        // anti troll mechanism methods
        //=======================================================================================
        private void CheckCreationTroll()
        {
            int creationCount = 0;
            foreach (Appointment appointment in PatientDataManager.AllAppointments)
            {
                if (appointment.HealthRecordID == signedUser.HealthRecordID)
                {
                    TimeSpan timePassedSinceScheduling = DateTime.Now.Subtract(appointment.CreatedDate);
                    if (timePassedSinceScheduling.TotalDays < 30)
                    {
                        ++creationCount;
                    }
                }
            }

            if (creationCount >= creationTrollLimit)
            {
                signedUser.IsBlocked = true;
                signedUser.BlockedBy = Enums.Blocker.System;

                MessageBox.Show("You have been blocked for excessive amounts of appointments created in the last 30 days");
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }

        private void CheckModificationTroll()
        {
            int modificationCount = 0;
            foreach (AppointmentChangeRequest changeRequest in PatientDataManager.AllChangeRequests)
            {
                if (changeRequest.PatientID == signedUser.ID)
                {
                    TimeSpan timePassedSinceScheduling = DateTime.Now.Subtract(changeRequest.DateSent);
                    if (timePassedSinceScheduling.TotalDays < 30)
                    {
                        ++modificationCount;
                    }
                }
            }

            if (modificationCount >= modificationTrollLimit)
            {
                signedUser.IsBlocked = true;
                signedUser.BlockedBy = Enums.Blocker.System;

                MessageBox.Show("You have been blocked for excessive amounts of change requests sent in the last 30 days");

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }
        //=======================================================================================

    }
}
