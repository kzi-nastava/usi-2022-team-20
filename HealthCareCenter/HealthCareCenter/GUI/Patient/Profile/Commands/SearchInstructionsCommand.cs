﻿using HealthCareCenter.Core.Medicine.Models;
using HealthCareCenter.Core.Medicine.Repositories;
using HealthCareCenter.Core.Medicine.Services;
using HealthCareCenter.Core.Prescriptions;
using HealthCareCenter.Core.Users;
using HealthCareCenter.Core.Users.Services;
using HealthCareCenter.GUI.Patient.Profile.ViewModels;
using HealthCareCenter.GUI.Patient.SharedCommands;
using HealthCareCenter.GUI.Patient.SharedViewModels;
using System.Collections.Generic;

namespace HealthCareCenter.GUI.Patient.Profile.Commands
{
    internal class SearchInstructionsCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            List<MedicineInstructionFromPrescriptionViewModel> instructions;
            if (string.IsNullOrEmpty(_viewModel.SearchKeyword))
            {
                // restarts the search and shows all patient's prescriptions

                instructions = new List<MedicineInstructionFromPrescriptionViewModel>();
                foreach (Prescription prescription in _viewModel.PatientPrescriptions)
                {
                    foreach (int instructionID in prescription.MedicineInstructionIDs)
                    {
                        instructions.Add(
                            new MedicineInstructionFromPrescriptionViewModel(
                                prescription,
                                _medicineInstructionService.GetSingle(instructionID),
                                new MedicineService(
                                    new MedicineRepository()),
                                new UserService(
                                    new UserRepository())));
                    }
                }

                _viewModel.Instructions = instructions;
                return;
            }

            instructions = new List<MedicineInstructionFromPrescriptionViewModel>();
            foreach (Prescription prescription in _viewModel.PatientPrescriptions)
            {
                foreach (int instructionID in prescription.MedicineInstructionIDs)
                {
                    MedicineInstruction instruction = _medicineInstructionService.GetSingle(instructionID);
                    if (_medicineService.GetName(instruction.MedicineID).ToLower().Contains(
                        _viewModel.SearchKeyword.Trim().ToLower()))
                    {
                        instructions.Add(
                            new MedicineInstructionFromPrescriptionViewModel(
                                prescription,
                                _medicineInstructionService.GetSingle(instructionID),
                                new MedicineService(
                                    new MedicineRepository()),
                                new UserService(
                                    new UserRepository())));
                    }
                }
            }

            _viewModel.Instructions = instructions;
        }

        private readonly MyPrescriptionsViewModel _viewModel;
        private readonly IMedicineInstructionService _medicineInstructionService;
        private readonly IMedicineService _medicineService;

        public SearchInstructionsCommand(
            MyPrescriptionsViewModel viewModel,
            IMedicineInstructionService medicineInstructionService,
            IMedicineService medicineService)
        {
            _viewModel = viewModel;
            _medicineInstructionService = medicineInstructionService;
            _medicineService = medicineService;
        }
    }
}
