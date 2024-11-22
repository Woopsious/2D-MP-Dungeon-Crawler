using Unity.VisualScripting.Antlr3.Runtime.Misc;

public interface IBossPhases
{
	public void PhaseOne();
	public void PhaseTwo();
	public void PhaseThree();
}

public interface IBossTransitionPhases
{
	public void PhaseOneTransition();
	public void PhaseTwoTransition();
	public void PhaseThreeTransition();
}

public interface IBossAbilities
{
	public bool CanUseBossAbilityOne();
	public bool CanUseBossAbilityTwo();
	public bool CanUseBossAbilityThree();

	public void PhaseTransitionOneSteps();
	public void PhaseTransitionTwoSteps();
	public void PhaseTransitionThreeSteps();

	public bool CanUseTransitionAbilityOne();
	public bool CanUseTransitionAbilityTwo();
	public bool CanUseTransitionAbilityThree();
}