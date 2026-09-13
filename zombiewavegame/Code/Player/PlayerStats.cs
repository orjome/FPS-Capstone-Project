using Sandbox;

public sealed class PlayerStats : Component
{
	[Property, Sync] public string DisplayName { get; set; } = "Player";
	[Property, Sync] public int Kills { get; set; } = 0;
	[Property, Sync] public int Deaths { get; set; } = 0;

	protected override void OnStart()
	{
		var owner = GameObject.Network.Owner;
		if ( owner is not null )
			DisplayName = owner.DisplayName;
	}
}
