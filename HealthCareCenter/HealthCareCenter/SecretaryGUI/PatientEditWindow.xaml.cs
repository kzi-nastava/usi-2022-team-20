﻿using HealthCareCenter.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HealthCareCenter.SecretaryGUI
{
    /// <summary>
    /// Interaction logic for PatientEditWindow.xaml
    /// </summary>
    public partial class PatientEditWindow : Window
    {
        private Patient _patient;
        private HealthRecord _record;

        private ObservableCollection<string> _previousDiseases;
        private ObservableCollection<string> _allergens;

        public PatientEditWindow()
        {
            InitializeComponent();
        }

        public PatientEditWindow(Patient patient, HealthRecord record)
        {
            this._patient = patient;
            this._record = record;
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
            previousDiseasesListBox.ItemsSource = null;
            allergensListBox.ItemsSource = null;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void AddPreviousDiseaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(previousDiseaseTextBox.Text))
            {
                MessageBox.Show("You must enter a disease.");
                return;
            }
            _previousDiseases.Add(previousDiseaseTextBox.Text);
        }

        private void AddAllergenButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(allergenTextBox.Text))
            {
                MessageBox.Show("You must enter an allergen.");
                return;
            }
            _allergens.Add(allergenTextBox.Text);
        }

        private void DeletePreviousDiseaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (previousDiseasesListBox.SelectedItem != null)
            {
                _previousDiseases.Remove(previousDiseasesListBox.SelectedItem.ToString());
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
                _allergens.Remove(allergensListBox.SelectedItem.ToString());
            }
            else
            {
                MessageBox.Show("You need to select an allergen from the list first.");
            }
        }

        private void EditPreviousDiseaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(previousDiseaseTextBox.Text))
            {
                MessageBox.Show("You must enter a disease.");
                return;
            }
            if (previousDiseasesListBox.SelectedItem != null)
            {
                _previousDiseases[previousDiseasesListBox.SelectedIndex] = previousDiseaseTextBox.Text;
            }
            else
            {
                MessageBox.Show("You need to select a disease from the list first.");
            }
        }

        private void EditAllergenButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(allergenTextBox.Text))
            {
                MessageBox.Show("You must enter an allergen.");
                return;
            }
            if (allergensListBox.SelectedItem != null)
            {
                _allergens[allergensListBox.SelectedIndex] = allergenTextBox.Text;
            }
            else
            {
                MessageBox.Show("You need to select an allergen from the list first.");
            }
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

        private void Window_Initialized(object sender, EventArgs e)
        {
            idTextBox.Text = _patient.ID.ToString();
            firstNameTextBox.Text = _patient.FirstName;
            usernameTextBox.Text = _patient.Username;
            lastNameTextBox.Text = _patient.LastName;
            passwordTextBox.Text = _patient.Password;
            birthDatePicker.SelectedDate = _patient.DateOfBirth;
            heightTextBox.Text = _record.Height.ToString();
            weightTextBox.Text = _record.Weight.ToString();
            _previousDiseases = new ObservableCollection<string>(_record.PreviousDiseases);
            previousDiseasesListBox.ItemsSource = _previousDiseases;
            _allergens = new ObservableCollection<string>(_record.Allergens);
            allergensListBox.ItemsSource = _allergens;
        }

        private bool EnteredData()
        {
            if (string.IsNullOrWhiteSpace(firstNameTextBox.Text))
            {
                MessageBox.Show("You must enter a first name.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(usernameTextBox.Text))
            {
                MessageBox.Show("You must enter a username.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(lastNameTextBox.Text))
            {
                MessageBox.Show("You must enter a last name.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(passwordTextBox.Text))
            {
                MessageBox.Show("You must enter a password.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(heightTextBox.Text))
            {
                MessageBox.Show("You must enter a height.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(weightTextBox.Text))
            {
                MessageBox.Show("You must enter a weight.");
                return false;
            }
            if (birthDatePicker.SelectedDate == null)
            {
                MessageBox.Show("You must enter a date of birth.");
                return false;
            }
            return true;
        }

        private bool BirthDateInFuture()
        {
            if (birthDatePicker.SelectedDate > DateTime.Now)
            {
                MessageBox.Show("You cannot enter a date in the future.");
                return true;
            }
            return false;
        }

        private bool ValidHeight()
        {
            if (!Double.TryParse(heightTextBox.Text, out double height))
            {
                MessageBox.Show("Height must be a number.");
                return false;
            }
            return true;
        }

        private bool ValidWeight()
        {
            if (!Double.TryParse(weightTextBox.Text, out double weight))
            {
                MessageBox.Show("Weight must be a number.");
                return false;
            }
            return true;
        }

        private bool UsernameInUse()
        {
            if (_patient.Username == usernameTextBox.Text)
            {
                return false;
            }
            foreach (User user in UserRepository.Users)
            {
                if (user.Username == usernameTextBox.Text)
                {
                    MessageBox.Show("Username is already in use. Choose a different one.");
                    return true;
                }
            }
            return false;
        }

        private bool ValidData()
        {
            if (BirthDateInFuture())
                return false;

            if (UsernameInUse())
                return false;

            if (!ValidHeight())
                return false;

            if (!ValidWeight())
                return false;

            return true;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (!EnteredData())
                return;

            if (!ValidData())
                return;

            EditHealthRecord();
            EditPatient();

            SaveToRepositories();

            MessageBox.Show("Successfully edited the patient and health record.");
        }

        private static void SaveToRepositories()
        {
            HealthRecordRepository.Save();
            UserRepository.SavePatients();
        }

        private void EditPatient()
        {
            _patient.Username = usernameTextBox.Text;
            _patient.Password = passwordTextBox.Text;
            _patient.FirstName = firstNameTextBox.Text;
            _patient.LastName = lastNameTextBox.Text;
            _patient.DateOfBirth = (DateTime)birthDatePicker.SelectedDate;
        }

        private void EditHealthRecord()
        {
            _record.Height = Double.Parse(heightTextBox.Text);
            _record.Weight = Double.Parse(weightTextBox.Text);
            _record.PreviousDiseases = _previousDiseases.Cast<String>().ToList();
            _record.Allergens = _allergens.Cast<String>().ToList();
        }
    }
}