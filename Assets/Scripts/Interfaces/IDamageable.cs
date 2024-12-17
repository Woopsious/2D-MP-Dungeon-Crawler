public interface IDamagable
{
	public enum HitBye
	{
		player, entity, enviroment
	}
	enum DamageType
	{
		isPhysicalDamage, isPoisonDamage, isFireDamage, isIceDamage, isHealing
	}
	void RecieveDamage(int damage, DamageType damageType, bool isPercentageValue);
}
