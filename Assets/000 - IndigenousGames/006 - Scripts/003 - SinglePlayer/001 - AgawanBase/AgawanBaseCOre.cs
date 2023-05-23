using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class AgawanBaseCOre : MonoBehaviour
{
    public enum MatchState
    {
        NONE,
        GETREADY,
        FINISH
    }
    private event EventHandler MatchStateChange;
    public event EventHandler OnMatchStateChange
    {
        add
        {
            if (MatchStateChange == null || !MatchStateChange.GetInvocationList().Contains(value))
                MatchStateChange += value;
        }
        remove { MatchStateChange -= value; }
    }
    public MatchState CurrentMatchState
    {
        get => currentMatchState;
        set
        {
            currentMatchState = value;
            MatchStateChange?.Invoke(this, EventArgs.Empty);
        }
    }

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private MatchState currentMatchState;
}
