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
    /// Interaction logic for ArrangingEquipmentWindow.xaml
    /// </summary>
    public partial class ArrangingEquipmentWindow : Window
    {
        private string[] _headerDataGridEquipment = new string[]
        {
            "Equipment Id", "Current Room Id", "Equipment Type",
            "Equipment Name", "Move Time", "New Room Id"
        };

        private Manager _signedManager;
        private ArrangingEquipmentController _controller = new ArrangingEquipmentController();

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

        /// <summary>
        /// Adding row in DataGrid.
        /// The row of the DataGrid is obtained by making a string array from object. For example if object is type of equipment and object has arrangement, then the date of arrangement and new room for equipment are added to array like strings, if equipment has no arragement then two empty strings are added to the string array which one represents the row.
        /// </summary>
        /// <param name="equipmentAttributesToDisplay">Content we want to display in DataGridEquipment (One row)</param>
        private void AddDataGridRow(DataGrid dataGrid, string[] header, List<string> equipmentAttributesToDisplay)
        {
            dynamic row = new ExpandoObject();

            for (int i = 0; i < header.Length; i++)
            {
                ((IDictionary<String, Object>)row)[header[i].Replace(' ', '_')] = equipmentAttributesToDisplay[i];
            }

            dataGrid.Items.Add(row);
        }

        /// <summary>
        /// When equipment object don't have rearrangement we add 2 empty strings for "Move Time" and for "New Room Id"
        /// </summary>
        /// <param name="equipmentAttributesToDisplay">Content we want to display in DataGridEquipment</param>
        private void AddEmptyFieldsForEquipmentDisplay(ref List<string> equipmentAttributesToDisplay)
        {
            equipmentAttributesToDisplay.Add("");
            equipmentAttributesToDisplay.Add("");
        }

        /// <summary>
        /// Filling DataGridEquipment with content
        /// </summary>
        private void FillDataGridEquipment()
        {
            List<Equipment> equipments = EquipmentService.GetEquipments();
            foreach (Equipment equipment in equipments)
            {
                if (!EquipmentService.HasScheduledRearrangement(equipment))
                {
                    List<string> equipmentAttributesToDisplay = equipment.ToList();
                    AddEmptyFieldsForEquipmentDisplay(ref equipmentAttributesToDisplay);
                    AddDataGridRow(DataGridEquipments, _headerDataGridEquipment, equipmentAttributesToDisplay);
                }
                else
                {
                    List<string> equipmentAttributesToDisplay = equipment.ToList();
                    EquipmentRearrangement rearrangement = EquipmentRearrangementService.Get(equipment.RearrangementID);
                    equipmentAttributesToDisplay.Add(rearrangement.MoveTime.ToString(Constants.DateFormat));
                    equipmentAttributesToDisplay.Add(rearrangement.NewRoomID.ToString());
                    AddDataGridRow(DataGridEquipments, _headerDataGridEquipment, equipmentAttributesToDisplay);
                }
            }
        }

        public ArrangingEquipmentWindow(Manager manager)
        {
            _signedManager = manager;
            InitializeComponent();
            TimeComboBox.SelectedItem = TimeComboBox.Items[0];
            AddDataGridHeader(DataGridEquipments, _headerDataGridEquipment);
            FillDataGridEquipment();
        }

        private void TransferButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newRoomId = NewRoomIdTextBox.Text;
                string equipmentForRearrangementId = EquipmentIdTextBox.Text;
                string rearrangementDate = DatePicker.Text;
                string rearrangementTime = TimeComboBox.Text;

                _controller.SetRearrangement(newRoomId, equipmentForRearrangementId, rearrangementDate, rearrangementTime);
                DataGridEquipments.Items.Clear();
                FillDataGridEquipment();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string equipmentForRearrangementId = EquipmentIdTextBox.Text;
                _controller.UndoRearrangement(equipmentForRearrangementId);

                DataGridEquipments.Items.Clear();
                FillDataGridEquipment();
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