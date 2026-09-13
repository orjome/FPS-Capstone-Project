using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class ZombieGameManager : Component
{
	[Property] public GameObject ZombiePrefab { get; set; }
	[Property] public List<GameObject> SpawnPoints { get; set; } = new();

	[Property, Sync] public int Round { get; set; } = 1;
	[Property, Sync] public int ZombiesRemaining { get; set; } = 0;

	int ZombiesThisRound => 4 + (Round * 2);

	protected override void OnStart()
	{
		Log.Info( $"GameManager starting - Spawn points: {SpawnPoints.Count} - Prefab: {ZombiePrefab}" );
		StartRound();
	}

	public void StartRound()
	{
		ZombiesRemaining = ZombiesThisRound;
		Log.Info( $"Round {Round} started! Spawning {ZombiesThisRound} zombies." );
		SpawnZombies( ZombiesThisRound );
	}

	void SpawnZombies( int count )
	{
		for ( int i = 0; i < count; i++ )
		{
			// Pick a random spawn point
			var spawnPoint = SpawnPoints[Game.Random.Int( 0, SpawnPoints.Count - 1 )];
			var zombie = ZombiePrefab.Clone( spawnPoint.WorldPosition );

			// Scale health with round
			var ai = zombie.GetComponent<ZombieAI>();
			if ( ai != null )
			{
				ai.Health = 100 + (Round * 50);
			}
		}
	}
	public void OnPlayerDied()
	{
		Log.Info( "Game Over!" );

		var playerXP = Scene.GetAllComponents<PlayerXP>().FirstOrDefault();
		var gameOver = Scene.GetAllComponents<GameOver>().FirstOrDefault();
		gameOver?.ShowGameOver( Round, playerXP?.Level ?? 1 );
	}

	public void OnZombieDied()
	{
		ZombiesRemaining--;
		Log.Info( $"Zombie died! {ZombiesRemaining} remaining." );

		if ( ZombiesRemaining <= 0 )
		{
			Log.Info( $"Round {Round} complete! Next round starting soon..." );
			Round++;
			_ = StartNextRound();
		}
	}

	async Task StartNextRound()
	{
		await Task.DelaySeconds( 5f );

		// Make sure we are back on the main thread before spawning
		await GameTask.MainThread();

		StartRound();
	}
}

