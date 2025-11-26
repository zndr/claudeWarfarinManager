using WarfarinManager.Data.Entities;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Core.Services;

public interface IBridgeTherapyService
{
    int CalculateCHA2DS2VAScScore(Patient patient, bool? hadStrokeTIA = null);
    
    ThromboembolicRisk DetermineThromboembolicRisk(
        Patient patient, 
        Indication? activeIndication,
        int? cha2ds2vascOverride = null);
    
    BleedingRisk DetermineBleedingRisk(SurgeryType surgeryType);
    
    BridgeRecommendation GetFCSARecommendation(
        ThromboembolicRisk teRisk,
        BleedingRisk bleedingRisk,
        bool hasMechanicalValve);
    
    BridgeRecommendation GetACCPRecommendation(
        ThromboembolicRisk teRisk,
        BleedingRisk bleedingRisk,
        bool hasMechanicalValve);
    
    BridgeProtocol GenerateProtocol(
        Patient patient,
        DateTime surgeryDate,
        SurgeryType surgeryType,
        ThromboembolicRisk teRisk,
        BleedingRisk bleedingRisk,
        GuidelineType guideline,
        bool bridgeRecommended,
        BridgeDosageType? dosageType = null);
    
    string FormatProtocolForExport(BridgeProtocol protocol, Patient patient);
}

public class BridgeRecommendation
{
    public bool BridgeRecommended { get; set; }
    public BridgeDosageType? DosageType { get; set; }
    public string Rationale { get; set; } = string.Empty;
    public string Warnings { get; set; } = string.Empty;
    public GuidelineType Guideline { get; set; }
}

public class BridgeProtocol
{
    public DateTime SurgeryDate { get; set; }
    public SurgeryType SurgeryType { get; set; }
    public ThromboembolicRisk TERisk { get; set; }
    public BleedingRisk BleedingRisk { get; set; }
    public int CHA2DS2VAScScore { get; set; }
    
    public bool BridgeRecommended { get; set; }
    public BridgeDosageType? DosageType { get; set; }
    public GuidelineType Guideline { get; set; }
    
    public DateTime WarfarinStopDate { get; set; }
    public DateTime? EBPMStartDate { get; set; }
    public DateTime? EBPMLastDoseDate { get; set; }
    public DateTime? INRCheckDate1 { get; set; }
    public DateTime? INRCheckDate2 { get; set; }
    public DateTime WarfarinResumeDate { get; set; }
    public DateTime? EBPMResumeDate { get; set; }
    
    public string? EBPMDrug { get; set; }
    public string? EBPMDosage { get; set; }
    public string? EBPMFrequency { get; set; }
    
    public string FCSARecommendation { get; set; } = string.Empty;
    public string? ACCPRecommendation { get; set; }
    public List<string> ClinicalNotes { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public enum BridgeDosageType
{
    None,
    Prophylactic,
    Therapeutic
}
