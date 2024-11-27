public interface IDamagable
{
	public enum HitBye
	{
		player, entity, enviroment
	}
	enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}
	void RecieveDamage(int damage, DamageType damageType, bool isPercentageValue);
}
