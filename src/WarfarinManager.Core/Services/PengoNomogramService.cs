using System;
using System.Collections.Generic;
using System.Linq;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Servizio per il calcolo del fabbisogno di warfarin secondo il Nomogramma di Pengo (2001)
/// </summary>
public class PengoNomogramService
{
    // Dati del nomogramma: INR -> Fabbisogno settimanale in mg
    private static readonly Dictionary<decimal, decimal> NomogramData = new()
    {
        { 1.0m, 71m },
        { 1.1m, 57m },
        { 1.2m, 48m },
        { 1.3m, 43m },
        { 1.4m, 39m },
        { 1.5m, 35m },
        { 1.6m, 33m },
        { 1.7m, 31m },
        { 1.8m, 29m },
        { 1.9m, 27m },
        { 2.0m, 26m },
        { 2.1m, 24m },
        { 2.2m, 23m },
        { 2.3m, 22m },
        { 2.4m, 21m },
        { 2.5m, 20m },
        { 2.6m, 19m },
        { 2.7m, 18m },
        { 2.8m, 17m },
        { 2.9m, 16.5m },
        { 3.0m, 16m },
        { 3.1m, 15m },
        { 3.2m, 14m },
        { 3.3m, 13.5m },
        { 3.4m, 13m },
        { 3.5m, 12m },
        { 3.6m, 11.5m },
        { 3.7m, 11m },
        { 3.8m, 10.5m },
        { 3.9m, 10m },
        { 4.0m, 9m },
        { 4.1m, 8.5m },
        { 4.2m, 8m },
        { 4.3m, 7.5m },
        { 4.4m, 7m }
    };

    /// <summary>
    /// Ottiene il fabbisogno settimanale stimato dal nomogramma di Pengo
    /// </summary>
    /// <param name="inrValue">Valore INR misurato al 5° giorno (dopo 4 dosi da 5mg)</param>
    /// <returns>Fabbisogno settimanale stimato in mg</returns>
    public decimal GetEstimatedWeeklyDose(decimal inrValue)
    {
        // Se INR esattamente nel nomogramma, ritorna il valore
        if (NomogramData.TryGetValue(inrValue, out var exactDose))
        {
            return exactDose;
        }

        // Se INR fuori range, usa il valore limite più vicino
        if (inrValue < 1.0m)
        {
            return NomogramData[1.0m]; // 71 mg
        }

        if (inrValue > 4.4m)
        {
            return NomogramData[4.4m]; // 7 mg
        }

        // Interpolazione lineare per valori intermedi
        var lowerKey = NomogramData.Keys.Where(k => k < inrValue).OrderByDescending(k => k).FirstOrDefault();
        var upperKey = NomogramData.Keys.Where(k => k > inrValue).OrderBy(k => k).FirstOrDefault();

        if (lowerKey == 0 || upperKey == 0)
        {
            // Fallback: arrotonda al più vicino
            var nearestKey = NomogramData.Keys.OrderBy(k => Math.Abs(k - inrValue)).First();
            return NomogramData[nearestKey];
        }

        var lowerDose = NomogramData[lowerKey];
        var upperDose = NomogramData[upperKey];

        // Interpolazione lineare
        var fraction = (inrValue - lowerKey) / (upperKey - lowerKey);
        var interpolatedDose = lowerDose + fraction * (upperDose - lowerDose);

        // Arrotonda a 0.5 mg
        return Math.Round(interpolatedDose * 2, MidpointRounding.AwayFromZero) / 2;
    }

    /// <summary>
    /// Applica l'arrotondamento clinico al fabbisogno stimato
    /// </summary>
    /// <param name="estimatedDose">Dose stimata dal nomogramma</param>
    /// <param name="patientAge">Età del paziente</param>
    /// <param name="hasBledScore">Punteggio HAS-BLED</param>
    /// <param name="inrValue">Valore INR corrente</param>
    /// <returns>Dose arrotondata secondo criteri clinici</returns>
    public decimal ApplyClinicalRounding(decimal estimatedDose, int patientAge, int hasBledScore, decimal inrValue)
    {
        // Criteri per arrotondamento per DIFETTO:
        // - età > 75 anni
        // - HAS-BLED ≥ 3
        // - INR > 2.5
        bool shouldRoundDown = patientAge > 75 || hasBledScore >= 3 || inrValue > 2.5m;

        if (shouldRoundDown)
        {
            // Arrotonda per difetto al 2.5 mg più vicino
            return Math.Floor(estimatedDose / 2.5m) * 2.5m;
        }
        else
        {
            // Arrotonda per eccesso al 2.5 mg più vicino
            return Math.Ceiling(estimatedDose / 2.5m) * 2.5m;
        }
    }

    /// <summary>
    /// Verifica se un valore INR è all'interno del range del nomogramma
    /// </summary>
    public bool IsInrInNomogramRange(decimal inrValue)
    {
        return inrValue >= 1.0m && inrValue <= 4.4m;
    }

    /// <summary>
    /// Ottiene il path dell'HTML del nomogramma
    /// </summary>
    public string GetNomogramHtmlPath()
    {
        return "docs/nomogramma-pengo.html";
    }

    /// <summary>
    /// Genera l'URL dell'HTML del nomogramma con evidenziazione dell'INR
    /// </summary>
    public string GetNomogramHtmlUrl(decimal inrValue)
    {
        var basePath = GetNomogramHtmlPath();
        return $"{basePath}?inr={inrValue:F1}";
    }
}
