using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class ArenaGameManager : Component, Component.INetworkListener
{
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public List<GameObject> SpawnPoints { get; set; } = new();
	[Property] public int KillLimit { get; set; } = 10;
	[Property] public float RespawnDelay { get; set; } = 3f;

	[Property, Sync] public bool MatchOver { get; set; } = false;
	[Property, Sync] public string WinnerName { get; set; } = "";

	public Transform GetRandomSpawnPoint()
	{
		var point = SpawnPoints[Game.Random.Int( 0, SpawnPoints.Count - 1 )];
		return point.WorldTransform;
	}

	public void OnActive( Connection channel )
	{
		if ( PlayerPrefab is null || SpawnPoints.Count == 0 ) return;

		var spawnPoint = GetRandomSpawnPoint();
		var player = PlayerPrefab.Clone( spawnPoint );
		player.Name = $"Player - {channel.DisplayName}";
		player.NetworkSpawn( channel );
	}

	public void OnDisconnected( Connection channel )
	{
		var player = Scene.GetAllComponents<PlayerHealth>()
			.FirstOrDefault( p => p.GameObject.Network.Owner == channel );

		player?.GameObject.Destroy();
	}

	public void OnPlayerKilled( GameObject victim, GameObject killer )
	{
		if ( MatchOver ) return;

		var victimStats = victim?.GetComponent<PlayerStats>();
		if ( victimStats != null )
			victimStats.Deaths++;

		if ( killer != null && killer != victim )
		{
			var killerStats = killer.GetComponent<PlayerStats>();
			if ( killerStats != null )
			{
				killerStats.Kills++;
				killer.GetComponent<PlayerXP>()?.AddXP( 100 );
				Log.Info( $"{killerStats.DisplayName} killed {victimStats?.DisplayName ?? "someone"}! ({killerStats.Kills}/{KillLimit})" );

				if ( killerStats.Kills >= KillLimit )
				{
					EndMatch( killerStats.DisplayName );
					return;
				}
			}
		}

		_ = RespawnPlayer( victim?.GetComponent<PlayerHealth>() );
	}

	async Task RespawnPlayer( PlayerHealth playerHealth )
	{
		if ( playerHealth is null ) return;

		await Task.DelaySeconds( RespawnDelay );
		await GameTask.MainThread();

		if ( !playerHealth.IsValid || MatchOver ) return;

		playerHealth.Respawn( GetRandomSpawnPoint() );
	}

	void EndMatch( string winnerName )
	{
		MatchOver = true;
		WinnerName = winnerName;
		Log.Info( $"Match over! {winnerName} wins!" );
	}
}
