﻿using HealthCareCenter.Core;
using HealthCareCenter.Core.Appointments.Repository;
using HealthCareCenter.Core.HealthRecords;
using HealthCareCenter.Core.Patients;
using HealthCareCenter.Core.Patients.Controllers;
using HealthCareCenter.Core.Patients.Services;
using HealthCareCenter.Core.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HealthCareCenter.Secretary
{
    /// <summary>
    /// Interaction logic for PatientCreateWindow.xaml
    /// </summary>
    public partial class PatientCreateWindow : Window
    {
        private readonly PatientCreateController _controller;
        private readonly BaseHealthRecordRepository _healthRecordRepository;
        private readonly BaseUserRepository _userRepository;

        public PatientCreateWindow(
            BaseHealthRecordRepository healthRecordRepository,
            BaseUserRepository userRepository)
        {
            _controller = new PatientCreateController(
                new PatientService(
                    new AppointmentRepository(),
                    new AppointmentChangeRequestRepository(),
                    new HealthRecordRepository(),
                    new HealthRecordService(
                        new HealthRecordRepository()),
                    new PatientEditService(
                        new HealthRecordRepository(),
                        new UserRepository()),
                    new UserRepository()),
                new UserRepository());
            _healthRecordRepository = healthRecordRepository;
            _userRepository = userRepository;

            InitializeComponent();
        }

        private void Reset()
        {
            firstNameTextBox.Clear();
            usernameTextBox.Clear();
            lastNameTextBox.Clear();
            passwordTextBox.Clear();
            heightTextBox.Clear();
            weightTextBox.Clear();
            previousDiseaseTextBox.Clear();
            allergenTextBox.Clear();
            previousDiseasesListBox.Items.Clear();
            allergensListBox.Items.Clear();
            idTextBox.Text = (_userRepository.LargestID + 1).ToString();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void AddPreviousDiseaseButton_Click(object sender, RoutedEventArgs e)
        {
            string disease;
            try
            {
                disease = _controller.ValidatePreviousDisease(previousDiseaseTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            previousDiseasesListBox.Items.Add(disease);
        }

        private void AddAllergenButton_Click(object sender, RoutedEventArgs e)
        {
            string allergen;
            try
            {
                allergen = _controller.ValidateAllergen(allergenTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            allergensListBox.Items.Add(allergen);
        }

        private void DeletePreviousDiseaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (previousDiseasesListBox.SelectedItem != null) 
            {
                previousDiseasesListBox.Items.Remove(previousDiseasesListBox.SelectedItem);
            }
            else
            {
                MessageBox.Show("You need to select a disease from the list first.");
            }
        }

        private void DeleteAllergenButton_Click(object sender, RoutedEventArgs e)
        {
            if (allergensListBox.SelectedItem != null)
            {
                allergensListBox.Items.Remove(allergensListBox.SelectedItem);
            }
            else
            {
                MessageBox.Show("You need to select an allergen from the list first.");
            }
        }

        private void EditPreviousDiseaseButton_Click(object sender, RoutedEventArgs e)
        {
            string disease;
            try
            {
                disease = _controller.ValidatePreviousDisease(previousDiseaseTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            if (previousDiseasesListBox.SelectedItem != null)
            {
                previousDiseasesListBox.Items[previousDiseasesListBox.SelectedIndex] = disease;
            }
            else
            {
                MessageBox.Show("You need to select a disease from the list first.");
            }
        }

        private void EditAllergenButton_Click(object sender, RoutedEventArgs e)
        {
            string allergen;
            try
            {
                allergen = _controller.ValidateAllergen(allergenTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            if (allergensListBox.SelectedItem != null)
            {
                allergensListBox.Items[allergensListBox.SelectedIndex] = allergen;
            }
            else
            {
                MessageBox.Show("You need to select an allergen from the list first.");
            }
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            idTextBox.Text = (_userRepository.LargestID + 1).ToString();
        }

        private void PreviousDiseasesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (previousDiseasesListBox.SelectedItem != null)
            {
                previousDiseaseTextBox.Text = previousDiseasesListBox.SelectedValue.ToString();
            }
            else
            {
                previousDiseaseTextBox.Clear();
            }
        }

        private void AllergensListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (allergensListBox.SelectedItem != null)
            {
                allergenTextBox.Text = allergensListBox.SelectedValue.ToString();
            }
            else
            {
                allergenTextBox.Clear();
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            HealthRecordDTO record = new HealthRecordDTO(_healthRecordRepository.LargestID + 1, heightTextBox.Text, weightTextBox.Text, previousDiseasesListBox.Items.Cast<String>().ToList(), allergensListBox.Items.Cast<String>().ToList(), _userRepository.LargestID + 1);
            PatientDTO patient = new PatientDTO(_userRepository.LargestID + 1, usernameTextBox.Text, passwordTextBox.Text, firstNameTextBox.Text, lastNameTextBox.Text, birthDatePicker.SelectedDate, false, Blocker.None, new List<int>(), _healthRecordRepository.LargestID + 1);

            try
            {
                _controller.Create(patient, record);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            Reset();
            MessageBox.Show("Successfully created the patient and health record.");
        }
    }
}
