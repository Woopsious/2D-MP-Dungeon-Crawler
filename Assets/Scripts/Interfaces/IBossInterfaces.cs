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
}