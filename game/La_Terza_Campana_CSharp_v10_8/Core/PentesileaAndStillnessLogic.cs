using System;
using System.Collections.Generic;

namespace LaTerzaCampana.Events;

/// <summary>
/// Codice C# puro, pronto per essere collegato a GameRoot in Godot.
/// NON include grafica, audio né modifiche al progetto esportato.
/// Chiamare OnTileChanged SOLO dopo uno spostamento riuscito.
/// Chiamare Advance SOLO quando la schermata di gioco è attiva.
/// </summary>
public enum EventSignal
{
    StartPentesileaIntro,
    StillnessWarning90,
    StillnessWarning110,
    StillnessDeath120
}

public sealed class PentesileaAndStillnessLogic
{
    public const int HouseStepsToIntro = 12;
    public const double WarningOneSeconds = 30;
    public const double WarningTwoSeconds = 45;
    public const double DeathSeconds = 60;
    public const string HouseMap = "casa_olmo";
    public const string PentesileaMetFlag = "pentesilea_met";

    private readonly ISet<string> _storyFlags;
    private string _map = "";
    private int _stepsInsideHouse;
    private double _stationarySeconds;
    private bool _introPending;
    private bool _warning90;
    private bool _warning110;
    private bool _deathTriggered;

    public PentesileaAndStillnessLogic(ISet<string> storyFlags)
        => _storyFlags = storyFlags ?? throw new ArgumentNullException(nameof(storyFlags));

    public int StepsInsideHouse => _stepsInsideHouse;
    public double StationarySeconds => _stationarySeconds;
    public bool IntroPending => _introPending;
    public bool DeathTriggered => _deathTriggered;

    public void EnterMap(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId))
            throw new ArgumentException("Missing map identifier", nameof(mapId));
        if (_map == mapId) return;
        _map = mapId;
        _stepsInsideHouse = 0;
        _introPending = false;
        ResetStillness();
    }

    /// <summary>
    /// Invoke after a successful position change, never on key press or failed collision.
    /// </summary>
    public EventSignal? OnTileChanged()
    {
        ResetStillness();
        if (_map != HouseMap || _storyFlags.Contains(PentesileaMetFlag) || _introPending)
            return null;
        _stepsInsideHouse++;
        if (_stepsInsideHouse < HouseStepsToIntro) return null;
        _introPending = true;
        return EventSignal.StartPentesileaIntro;
    }

    public void FinishPentesileaIntro(int choice)
    {
        if (!_introPending) throw new InvalidOperationException("Pentesilea intro is not active");
        if (choice is < 1 or > 3) throw new ArgumentOutOfRangeException(nameof(choice));
        _storyFlags.Add(PentesileaMetFlag);
        _storyFlags.Add(choice switch
        {
            1 => "pentesilea_trust_high",
            2 => "pentesilea_trust_neutral",
            _ => "pentesilea_trust_low"
        });
        _introPending = false;
        _stepsInsideHouse = 0;
        ResetStillness();
    }

    /// <summary>
    /// Only advance when actually in exploration (not menus, cutscenes, dialogue,
    /// combat, pause, map transition, debug editor or unfocused window).
    /// Pausing does not reset the clock.
    /// </summary>
    public IReadOnlyList<EventSignal> Advance(double deltaSeconds, bool activeExploration)
    {
        var signals = new List<EventSignal>(3);
        if (!activeExploration || _introPending || _deathTriggered || deltaSeconds <= 0)
            return signals;

        _stationarySeconds += deltaSeconds;
        if (!_warning90 && _stationarySeconds >= WarningOneSeconds)
        {
            _warning90 = true;
            signals.Add(EventSignal.StillnessWarning90);
        }
        if (!_warning110 && _stationarySeconds >= WarningTwoSeconds)
        {
            _warning110 = true;
            signals.Add(EventSignal.StillnessWarning110);
        }
        if (_stationarySeconds >= DeathSeconds)
        {
            _deathTriggered = true;
            signals.Add(EventSignal.StillnessDeath120);
        }
        return signals;
    }

    public void ResetAfterLoadingGame()
    {
        _introPending = false;
        _stepsInsideHouse = 0;
        ResetStillness();
    }

    private void ResetStillness()
    {
        _stationarySeconds = 0;
        _warning90 = _warning110 = _deathTriggered = false;
    }
}
