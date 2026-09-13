using Sandbox;
using System.Collections.Generic;

public sealed class WeaponManager : Component
{
	[Property] public WeaponBase StartingPistol { get; set; }
	[Property, Sync] public int CurrentWeaponIndex { get; set; } = 0;

	public List<WeaponBase> Weapons { get; set; } = new();

	public WeaponBase CurrentWeapon => Weapons.Count > 0 ? Weapons[CurrentWeaponIndex] : null;

	protected override void OnStart()
	{
		// Always add pistol as slot 1
		if ( StartingPistol != null )
		{
			Weapons.Add( StartingPistol );
		}

		// Fill remaining slots with null
		Weapons.Add( null ); // slot 2
		Weapons.Add( null ); // slot 3

		EquipWeapon( 0 );
	}

	protected override void OnUpdate()
	{
		// Scroll wheel switching
		if ( Input.MouseWheel.y > 0 )
		{
			SwitchWeapon( 1 );
		}
		else if ( Input.MouseWheel.y < 0 )
		{
			SwitchWeapon( -1 );
		}

		// Number key switching
		if ( Input.Pressed( "slot1" ) ) EquipWeapon( 0 );
		if ( Input.Pressed( "slot2" ) ) EquipWeapon( 1 );
		if ( Input.Pressed( "slot3" ) ) EquipWeapon( 2 );
	}

	void SwitchWeapon( int direction )
	{
		int newIndex = (CurrentWeaponIndex + direction + Weapons.Count) % Weapons.Count;
		EquipWeapon( newIndex );
	}

	public void EquipWeapon( int index )
	{
		if ( index >= Weapons.Count ) return;
		if ( Weapons[index] == null ) return;

		foreach ( var weapon in Weapons )
		{
			if ( weapon != null )
			{
				weapon.Enabled = false;
				if ( weapon.ViewModel != null ) weapon.ViewModel.Enabled = false;
			}
		}

		CurrentWeaponIndex = index;
		Weapons[index].Enabled = true;
		if ( Weapons[index].ViewModel != null ) Weapons[index].ViewModel.Enabled = true;
	}

	public bool PickupWeapon( WeaponBase newWeapon )
	{
		// Check for empty slot first (slot 2 or 3)
		for ( int i = 1; i < Weapons.Count; i++ )
		{
			if ( Weapons[i] == null )
			{
				Weapons[i] = newWeapon;
				newWeapon.GameObject.SetParent( GameObject );
				newWeapon.Enabled = false;
				Log.Info( $"Picked up {newWeapon.WeaponName} into slot {i + 1}" );
				EquipWeapon( i );
				return true;
			}
		}

		// No empty slot — swap with current weapon (never swap slot 1 pistol)
		int swapIndex = CurrentWeaponIndex == 0 ? 1 : CurrentWeaponIndex;
		var oldWeapon = Weapons[swapIndex];

		// Drop old weapon back into the world
		if ( oldWeapon != null )
		{
			oldWeapon.GameObject.SetParent( null );
			oldWeapon.Enabled = true;
			Log.Info( $"Dropped {oldWeapon.WeaponName}" );
		}

		Weapons[swapIndex] = newWeapon;
		newWeapon.GameObject.SetParent( GameObject );
		newWeapon.Enabled = false;
		EquipWeapon( swapIndex );
		return true;
	}

	public string GetSlotName( int index )
	{
		if ( index >= Weapons.Count || Weapons[index] == null )
			return "Empty";
		return Weapons[index].WeaponName;
	}

	public void ResetForRespawn()
	{
		// Drop any picked-up weapons, keep the starting pistol only
		for ( int i = 1; i < Weapons.Count; i++ )
		{
			if ( Weapons[i] != null )
			{
				Weapons[i].GameObject.Destroy();
				Weapons[i] = null;
			}
		}

		StartingPistol?.RefillAmmo();
		EquipWeapon( 0 );
	}
}
