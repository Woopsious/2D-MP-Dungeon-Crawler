public interface IDamagable
{
	enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}
	void RecieveDamage(int damage, DamageType damageType);
}
