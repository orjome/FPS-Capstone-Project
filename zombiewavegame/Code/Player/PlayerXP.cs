using Sandbox;

public sealed class PlayerXP : Component
{
	[Property, Sync] public int XP { get; set; } = 0;
	[Property, Sync] public int TotalXP { get; set; } = 0;
	[Property, Sync] public int Level { get; set; } = 1;
	[Property, Sync] public int LevelXP { get; set; } = 0;

	public int XPToNextLevel => Level * 500;

	public void AddXP( int amount )
	{
		XP += amount;
		TotalXP += amount;
		LevelXP += amount;

		Log.Info( $"Gained {amount} XP! Spendable: {XP} | Level Progress: {LevelXP}/{XPToNextLevel}" );

		while ( LevelXP >= XPToNextLevel )
		{
			LevelXP -= XPToNextLevel;
			Level++;
			OnLevelUp();
		}
	}

	void OnLevelUp()
	{
		Log.Info( $"LEVEL UP! Now Level {Level}" );
	}
}
