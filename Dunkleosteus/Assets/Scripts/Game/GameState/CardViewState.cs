using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardViewState : FiniteState
{
    public GameDirector gameDirector;

    public CardViewState(GameDirector director)
    {
        gameDirector = director;
        _stateID = StateID.CardView;
    }

    public override void DoBeforeEnter() 
    {
        gameDirector.EnterCardViewState();
    }

    public override void DoBeforeExit()
    {
        gameDirector.ExitCardViewState();
    }
}