﻿using HealthCareCenter.Controller;
using HealthCareCenter.Model;
using HealthCareCenter.Service;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HealthCareCenter
{
    /// <summary>
    /// Interaction logic for ChangeMedicineRequestWindow.xaml
    /// </summary>
    public partial class ChangedMedicineCreationRequestWindow : Window
    {
        private Manager _signedManager;
        private string[] _headerOfChangedRequestsDataGrid = { "ID", "Medicine Name" };
        private string[] _headerOfIngredientsDataGrid = { "Ingredient Name" };
        private ChangedMedicineCreationRequestController _controller = new ChangedMedicineCreationRequestController();

        private List<string> CreateRowForDataGridIngredients(string ingredient)
        {
            List<string> row = new List<string>();
            row.Add(ingredient);
            return row;
        }

        private void AddDataGridHeader(DataGrid dataGrid, string[] header)
        {
            foreach (string label in header)
            {
                DataGridTextColumn column = new DataGridTextColumn();
                column.Header = label;
                column.Binding = new Binding(label.Replace(' ', '_'));
                column.Width = 110;
                dataGrid.Columns.Add(column);
            }
        }

        private void AddDataGridRow(DataGrid dataGrid, string[] header, List<string> equipmentAttributesToDisplay)
        {
            dynamic row = new ExpandoObject();

            for (int i = 0; i < header.Length; i++)
            {
                ((IDictionary<String, Object>)row)[header[i].Replace(' ', '_')] = equipmentAttributesToDisplay[i];
            }

            dataGrid.Items.Add(row);
        }

        private void FillDataGridChangedRequests()
        {
            DataGridChangedRequests.Items.Clear();
            foreach (MedicineCreationRequest request in MedicineCreationRequestRepository.Requests)
            {
                if (request.State == Enums.RequestState.Denied)
                {
                    List<string> row = new List<string>();
                    row.Add(request.ID.ToString());
                    row.Add(request.Name);
                    AddDataGridRow(DataGridChangedRequests, _headerOfChangedRequestsDataGrid, row);
                }
            }
        }

        private void FillDataGridIngredients()
        {
            DataGridIngredients.Items.Clear();
            foreach (string ingredient in _controller.AddedIngrediens)
            {
                List<string> row = CreateRowForDataGridIngredients(ingredient);
                AddDataGridRow(DataGridIngredients, _headerOfIngredientsDataGrid, row);
            }
        }

        private void DisplayRequestContentInTextBoxes()
        {
            MedicineNameTextBox.Text = _controller.DisplayedRequest.Name;
            ManufacturerTextBox.Text = _controller.DisplayedRequest.Manufacturer;
            CommentTextBlock.Text = _controller.DisplayedRequest.DenyComment;
        }

        private void EnableComponentsAfterDispaly()
        {
            IngredientTextBox.IsEnabled = true;
            AddIngredientButton.IsEnabled = true;
            RemoveIngredientButton.IsEnabled = true;
            SendButton.IsEnabled = true;
            MedicineNameTextBox.IsEnabled = true;
            ManufacturerTextBox.IsEnabled = true;
        }

        private void DisableComponentsAfterSend()
        {
            IngredientTextBox.IsEnabled = false;
            AddIngredientButton.IsEnabled = false;
            RemoveIngredientButton.IsEnabled = false;
            SendButton.IsEnabled = false;
            MedicineNameTextBox.IsEnabled = false;
            ManufacturerTextBox.IsEnabled = false;
        }

        private void ClearAllElements()
        {
            ChangeRequestIdTextBox.Text = "";
            MedicineNameTextBox.Text = "";

            MedicineNameTextBox.Text = "";
            IngredientTextBox.Text = "";
            ManufacturerTextBox.Text = "";
            CommentTextBlock.Text = "";
            DataGridIngredients.Items.Clear();
        }

        public ChangedMedicineCreationRequestWindow(Manager manager)
        {
            _signedManager = manager;
            InitializeComponent();
            FillDataGridChangedRequests();

            AddDataGridHeader(DataGridChangedRequests, _headerOfChangedRequestsDataGrid);
            AddDataGridHeader(DataGridIngredients, _headerOfIngredientsDataGrid);

            CommentTextBlock.IsEnabled = false;
            IngredientTextBox.IsEnabled = false;
            AddIngredientButton.IsEnabled = false;
            RemoveIngredientButton.IsEnabled = false;
            SendButton.IsEnabled = false;
            MedicineNameTextBox.IsEnabled = false;
            ManufacturerTextBox.IsEnabled = false;
        }

        private void DisplayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string changeRequistId = ChangeRequestIdTextBox.Text;
                _controller.DisplayRequest(changeRequistId);

                EnableComponentsAfterDispaly();
                FillDataGridIngredients();
                DisplayRequestContentInTextBoxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddIngredientButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ingredient = IngredientTextBox.Text;
                _controller.AddIngredient(ingredient);
                List<string> row = CreateRowForDataGridIngredients(ingredient);
                AddDataGridRow(DataGridIngredients, _headerOfIngredientsDataGrid, row);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RemoveIngredientButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ingredient = IngredientTextBox.Text;
                _controller.RemoveIngredient(ingredient);
                FillDataGridIngredients();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string medicineName = MedicineNameTextBox.Text;
                string manufacturer = ManufacturerTextBox.Text;
                _controller.Send(medicineName, manufacturer);
                ClearAllElements();
                DisableComponentsAfterSend();
                FillDataGridChangedRequests();

                MessageBox.Show("You have successfully send medicine on review!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ShowWindow(Window window)
        {
            window.Show();
            Close();
        }

        private void CrudHospitalRoomMenuItemClick(object sender, RoutedEventArgs e)
        {
            ShowWindow(new CrudHospitalRoomWindow(_signedManager));
        }

        private void EquipmentReviewMenuItemClick(object sender, RoutedEventArgs e)
        {
            ShowWindow(new HospitalEquipmentReviewWindow(_signedManager));
        }

        private void ArrangingEquipmentItemClick(object sender, RoutedEventArgs e)
        {
            ShowWindow(new ArrangingEquipmentWindow(_signedManager));
        }

        private void SimpleRenovationItemClick(object sender, RoutedEventArgs e)
        {
            ShowWindow(new HospitalRoomRenovationWindow(_signedManager));
        }

        private void ComplexRenovationMergeItemClick(object sender, RoutedEventArgs e)
        {
            ShowWindow(new ComplexHospitalRoomRenovationMergeWindow(_signedManager));
        }

        private void ComplexRenovationSplitItemClick(object sender, RoutedEventArgs e)
        {
            ShowWindow(new ComplexHospitalRoomRenovationSplitWindow(_signedManager));
        }

        private void CreateMedicineClick(object sender, RoutedEventArgs e)
        {
            ShowWindow(new MedicineCreationRequestWindow(_signedManager));
        }

        private void ReffusedMedicineClick(object sender, RoutedEventArgs e)
        {
            ShowWindow(new ChangedMedicineCreationRequestWindow(_signedManager));
        }

        private void LogOffItemClick(object sender, RoutedEventArgs e)
        {
            ShowWindow(new LoginWindow());
        }
    }
}