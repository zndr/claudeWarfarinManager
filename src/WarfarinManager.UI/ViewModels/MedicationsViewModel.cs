using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WarfarinManager.Core.Services;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.Shared.Enums;
using WarfarinManager.UI.Models;

namespace WarfarinManager.UI.ViewModels
{
    public partial class MedicationsViewModel : ObservableObject
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Data.Repositories.Interfaces.IInteractionDrugRepository _drugRepository;

        public MedicationsViewModel(IUnitOfWork unitOfWork, Data.Repositories.Interfaces.IInteractionDrugRepository drugRepository)
            {
            _unitOfWork = unitOfWork;
            _drugRepository = drugRepository;
            }
        private int _patientId;

        [ObservableProperty] private ObservableCollection<MedicationDto> _activeMedications = new();
        [ObservableProperty] private ObservableCollection<MedicationDto> _inactiveMedications = new();
        [ObservableProperty] private MedicationDto? _selectedMedication;
        [ObservableProperty] private bool _hasActiveMedications;
        [ObservableProperty] private bool _hasNoActiveMedications = true;
        [ObservableProperty] private bool _hasHighRiskInteractions;
        [ObservableProperty] private int _highRiskCount;
        [ObservableProperty] private int _mediumRiskCount;
        [ObservableProperty] private bool _showInactiveMedications;
        [ObservableProperty] private bool _isAddingMedication;
        [ObservableProperty] private string _newMedicationName = string.Empty;
        [ObservableProperty] private string _newDosage = string.Empty;
        [ObservableProperty] private string _newFrequency = string.Empty;
        [ObservableProperty] private DateTime _newStartDate = DateTime.Today;
        [ObservableProperty] private string _formErrorMessage = string.Empty;
        [ObservableProperty] private bool _hasFormError;

        public MedicationsViewModel(IUnitOfWork unitOfWork, IInteractionCheckerService interactionChecker)
        {
            _unitOfWork = unitOfWork;
            _interactionChecker = interactionChecker;
            
            System.Windows.MessageBox.Show("✅ MedicationsViewModel COSTRUTTORE chiamato!");
            System.Diagnostics.Debug.WriteLine("✅ MedicationsViewModel creato!");
        }

        public async Task LoadMedicationsAsync(int patientId)
        {
            _patientId = patientId;
            System.Diagnostics.Debug.WriteLine($"📊 LoadMedicationsAsync - PatientId: {patientId}");

            var medications = await _unitOfWork.Database.Medications
                .Where(m => m.PatientId == patientId)
                .OrderBy(m => m.MedicationName)
                .ToListAsync();
                
            System.Diagnostics.Debug.WriteLine($"📊 Trovati {medications.Count} farmaci totali");
            
            var dtos = medications.Select(MapToDto).ToList();

            ActiveMedications = new ObservableCollection<MedicationDto>(
                dtos.Where(m => m.IsActive).OrderBy(m => m.MedicationName));
            InactiveMedications = new ObservableCollection<MedicationDto>(
                dtos.Where(m => !m.IsActive).OrderByDescending(m => m.EndDate));

            HasActiveMedications = ActiveMedications.Any();
            HasNoActiveMedications = !HasActiveMedications;

            System.Diagnostics.Debug.WriteLine($"✅ Farmaci attivi: {ActiveMedications.Count}, Inattivi: {InactiveMedications.Count}");

            CalculateInteractionStats();
        }

        [RelayCommand]
        private void OpenAddMedicationDialog()
        {
            System.Diagnostics.Debug.WriteLine("➕ OpenAddMedicationDialog chiamato");
            IsAddingMedication = true;
            ClearForm();
        }

        [RelayCommand]
        private async Task SaveMedicationAsync()
            {
            System.Windows.MessageBox.Show("PULSANTE CLICCATO!");
            System.Diagnostics.Debug.WriteLine($"💾 INIZIO SaveMedicationAsync - PatientId: {_patientId}, Nome: '{NewMedicationName}'");

            if (string.IsNullOrWhiteSpace(NewMedicationName))
                {
                ShowFormError("Il nome del farmaco è obbligatorio");
                return;
                }

            if (_patientId == 0)
                {
                ShowFormError("Errore: ID paziente non valido");
                return;
                }

            try
                {
                System.Diagnostics.Debug.WriteLine("🔍 Controllo interazioni...");
                var drug = await _drugRepository.FindByNameAsync(NewMedicationName);
                InteractionLevel = interactionLevel,
                InteractionDetails = interactionDetails                string? interactionDetails = null;

                if (drug != null)
                    {
                    interactionLevel = drug.InteractionLevel;
                    interactionDetails = $"{drug.Mechanism}\n{drug.FCSAManagement}";
                    }
                System.Diagnostics.Debug.WriteLine($"   Livello: {interactionResult.InteractionLevel}");

                var medication = new Data.Entities.Medication
                    {
                    PatientId = _patientId,
                    MedicationName = NewMedicationName.Trim(),
                    Dosage = string.IsNullOrWhiteSpace(NewDosage) ? null : NewDosage.Trim(),
                    Frequency = string.IsNullOrWhiteSpace(NewFrequency) ? null : NewFrequency.Trim(),
                    StartDate = NewStartDate,
                    IsActive = true,
                    InteractionLevel = interactionResult.InteractionLevel,
                    InteractionDetails = interactionResult.HasInteraction
                        ? $"{interactionResult.Mechanism}\n{interactionResult.FCSAManagement}"
                        : null
                    };

                System.Diagnostics.Debug.WriteLine("💾 Salvataggio...");
                await _unitOfWork.Database.Medications.AddAsync(medication);
                await _unitOfWork.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("✅ SALVATO!");

                await LoadMedicationsAsync(_patientId);
                IsAddingMedication = false;
                ClearForm();
                }
            catch (Exception ex)
                {
                System.Diagnostics.Debug.WriteLine($"❌ ERRORE: {ex.Message}");
                ShowFormError($"Errore: {ex.Message}");
                }
            }

        [RelayCommand]
        private void CancelAddMedication()
        {
            IsAddingMedication = false;
            ClearForm();
        }

        [RelayCommand]
        private async Task StopMedicationAsync(MedicationDto medication)
        {
            if (medication == null) return;

            var entity = await _unitOfWork.Database.Medications.FindAsync(medication.Id);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.EndDate = DateTime.Today;
                _unitOfWork.Database.Medications.Update(entity);
                await _unitOfWork.SaveChangesAsync();
                await LoadMedicationsAsync(_patientId);
            }
        }

        [RelayCommand]
        private void ToggleInactiveMedications()
        {
            ShowInactiveMedications = !ShowInactiveMedications;
        }

        [RelayCommand]
        private void ShowInteractionDetails(MedicationDto medication)
        {
            if (medication == null) return;
            SelectedMedication = medication;
        }

        private void CalculateInteractionStats()
        {
            HighRiskCount = ActiveMedications.Count(m => m.InteractionLevel == "High");
            MediumRiskCount = ActiveMedications.Count(m => m.InteractionLevel == "Moderate");
            HasHighRiskInteractions = HighRiskCount > 0;
        }

        private void ClearForm()
        {
            NewMedicationName = string.Empty;
            NewDosage = string.Empty;
            NewFrequency = string.Empty;
            NewStartDate = DateTime.Today;
            FormErrorMessage = string.Empty;
            HasFormError = false;
        }

        private void ShowFormError(string message)
        {
            FormErrorMessage = message;
            HasFormError = true;
        }

        private MedicationDto MapToDto(Data.Entities.Medication entity)
        {
            return new MedicationDto
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                MedicationName = entity.MedicationName,
                Dosage = entity.Dosage ?? string.Empty,
                Frequency = entity.Frequency ?? string.Empty,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                IsActive = entity.IsActive,
                InteractionLevel = entity.InteractionLevel.ToString(), // Converti enum a stringa
                InteractionDetails = entity.InteractionDetails ?? string.Empty
            };
        }
    }
}
