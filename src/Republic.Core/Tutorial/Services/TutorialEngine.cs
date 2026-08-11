namespace Republic.Core.Tutorial.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.Diagnostics;
using Republic.Core.Tutorial.Models;

/// <summary>
/// Engine guiding first-time players through presidential onboarding steps.
/// </summary>
public sealed class TutorialEngine
{
    private readonly List<TutorialStep> _steps = new();
    private readonly ILogger? _logger;
    private int _currentStepIndex = 0;

    public TutorialEngine(ILogger? logger = null)
    {
        _logger = logger;
        RegisterDefaultSteps();
    }

    private void RegisterDefaultSteps()
    {
        _steps.Add(new TutorialStep { StepIndex = 0, Title = "Executive Suite", Instructions = "Welcome Mr. President. Review your daily inbox and hotline phone.", TargetElementId = "hotline_phone" });
        _steps.Add(new TutorialStep { StepIndex = 1, Title = "Cabinet Appointments", Instructions = "Appoint competent ministers to lead Finance and Defense portfolios.", TargetElementId = "cabinet_roster" });
        _steps.Add(new TutorialStep { StepIndex = 2, Title = "National Economy", Instructions = "Review treasury tax rates and regional infrastructure investment.", TargetElementId = "tax_policy" });
        _steps.Add(new TutorialStep { StepIndex = 3, Title = "Defense & Command", Instructions = "Monitor DEFCON alert levels and military branch readiness.", TargetElementId = "military_command" });
    }

    public TutorialStep? GetCurrentStep()
    {
        if (_currentStepIndex >= 0 && _currentStepIndex < _steps.Count)
        {
            return _steps[_currentStepIndex];
        }
        return null;
    }

    public bool AdvanceStep()
    {
        if (_currentStepIndex < _steps.Count)
        {
            _steps[_currentStepIndex].IsCompleted = true;
            _currentStepIndex++;
            _logger?.LogInfo($"[Tutorial] Advanced to step {_currentStepIndex}.");
            return true;
        }
        return false;
    }

    public bool IsTutorialFinished => _currentStepIndex >= _steps.Count;
}
