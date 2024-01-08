public interface IGetStatModifier
{
	enum Rarity
	{
		isCommon, isRare, isEpic, isLegendary
	}
	void GetStatModifier(int thingsLevel, Rarity rarity);
}
