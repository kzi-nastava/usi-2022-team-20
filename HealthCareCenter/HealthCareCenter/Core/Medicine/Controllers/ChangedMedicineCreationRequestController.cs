﻿using System;
using System.Collections.Generic;
using System.Text;
using HealthCareCenter.Core.Medicine.Services;
using HealthCareCenter.Core.Medicine.Models;
using HealthCareCenter.Core.Medicine.Repositories;
using HealthCareCenter.Core.Medicine.Exceptions;

namespace HealthCareCenter.Core.Medicine.Controllers
{
    public class ChangedMedicineCreationRequestController : BaseMedicineCreationRequestController
    {
        private IMedicineCreationRequestService _medicineCreationRequestService;
        private BaseMedicineCreationRequestRepository _medicineCreationRequestRepository;

        public ChangedMedicineCreationRequestController(IMedicineCreationRequestService medicineCreationRequestService, BaseMedicineCreationRequestRepository medicineCreationRequestRepository)
        {
            _medicineCreationRequestService = medicineCreationRequestService;
            _medicineCreationRequestRepository = medicineCreationRequestRepository;
        }

        public MedicineCreationRequest DisplayedRequest { get; set; }

        public override void Send(string medicineName, string manufacturer)
        {
            IsPossibleToCreateMedicineCreationRequest(medicineName, manufacturer);
            MedicineCreationRequest request = new MedicineCreationRequest(
              DisplayedRequest.ID, medicineName,
              DisplayedRequest.Ingredients, manufacturer,
              RequestState.Waiting);
            SendRequest(request);

            AddedIngrediens.Clear();
            DisplayedRequest = null;
        }

        public void DisplayRequest(string requestId)
        {
            IsRequestValide(requestId);
            int parsedChangeRequestId = Convert.ToInt32(requestId);
            DisplayedRequest = _medicineCreationRequestService.Get(parsedChangeRequestId);
            AddedIngrediens = DisplayedRequest.Ingredients;
        }

        public List<List<string>> GetRequestsForDisplay()
        {
            List<List<string>> requestsAttributesForDisplay = new List<List<string>>();
            foreach (MedicineCreationRequest request in _medicineCreationRequestRepository.Requests)
            {
                if (request.State == RequestState.Denied)
                {
                    List<string> attributes = new List<string>();
                    attributes.Add(request.ID.ToString());
                    attributes.Add(request.Name);
                    requestsAttributesForDisplay.Add(attributes);
                }
            }

            return requestsAttributesForDisplay;
        }

        private void SendRequest(MedicineCreationRequest request)
        {
            _medicineCreationRequestService.Delete(request);
            _medicineCreationRequestService.Add(request);
        }

        private bool IsRequestIdValide(string id)
        {
            return int.TryParse(id, out _);
        }

        private bool IsRequestFound(string id)
        {
            int parsedId = Convert.ToInt32(id);
            MedicineCreationRequest request = _medicineCreationRequestService.Get(parsedId);

            if (request == null) { return false; }
            else if (request.State != RequestState.Denied) { return false; }

            return true;
        }

        private void IsRequestValide(string id)
        {
            if (!IsRequestIdValide(id)) { throw new MedicineCreationRequestNotFoundException(id); }

            if (!IsRequestFound(id)) { throw new MedicineCreationRequestNotFoundException(id); }
        }
    }
}