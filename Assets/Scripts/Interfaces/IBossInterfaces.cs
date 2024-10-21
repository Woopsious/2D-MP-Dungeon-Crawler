using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public interface IBossPhases
{
	public void RunPhases();
	public void PhaseOne();
	public void PhaseTwo();
	public void PhaseThree();
}

public interface IBossTransitionPhases
{
	public void RunTransitions();
	public void PhaseOneTransition();
	public void PhaseTwoTransition();
	public void PhaseThreeTransition();
}

public interface IBossAbilities
{
	public bool CanUseBossAbilityOne();
	public bool CanUseBossAbilityTwo();
	public bool CanUseBossAbilityThree();
}
