using System;
using System.Windows.Forms;
using FiringSquad;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;

public class Main : Script
{
    // State variables to track the status of the script
    public static bool IsSpawned = false; // Indicates if the firing squad is currently spawned
    public static bool IsAiming = false;  // Indicates if the soldiers are aiming at the prisoner
    public static bool IsFired = false;   // Indicates if the soldiers have fired

    // Ped variables for the commander, soldiers, and prisoner
    public static Ped Sgt;
    public static Ped Soldier1;
    public static Ped Soldier2;
    public static Ped Soldier3;
    public static Ped Soldier4;
    public static Ped Soldier5;
    public static Ped Prisoner1;

    // Key bindings for summoning and firing the squad
    private Keys fireKey;
    private Keys summonKey;

    // Model variables for the soldier and commander models
    public static Model soldierModel1;
    public static Model soldierModel2;
    public static Model soldierModel3;
    public static Model soldierModel4;
    public static Model soldierModel5;
    public static Model commanderModel;

    // Constructor to initialize event handlers and script settings
    public Main()
    {
        Tick += OnTick;            // Event handler for the script's tick event
        KeyDown += OnKeyDown;      // Event handler for key down events
        KeyUp += OnKeyUp;          // Event handler for key up events
        Interval = 1;              // Interval for the tick event (1 ms)
    }

    // Event handler that runs on every tick of the script
    private void OnTick(object sender, EventArgs eventArgs)
    {
        // Load settings from the configuration file "FiringSquad.ini"
        ScriptSettings settings = ScriptSettings.Load("FiringSquad.ini");
        fireKey = (Keys)Enum.Parse(typeof(Keys), settings.GetValue<string>("Controls", "FireKey", "J"));  // Key for firing the squad
        summonKey = (Keys)Enum.Parse(typeof(Keys), settings.GetValue<string>("Controls", "SummonKey", "K"));  // Key for summoning the squad

        // Load soldier models from the settings
        soldierModel1 = settings.GetValue<Model>("SoldierModels", "Soldier1", new Model("s_m_y_cop_01"));
        soldierModel2 = settings.GetValue<Model>("SoldierModels", "Soldier2", new Model("s_m_y_cop_01"));
        soldierModel3 = settings.GetValue<Model>("SoldierModels", "Soldier3", new Model("s_m_y_cop_01"));
        soldierModel4 = settings.GetValue<Model>("SoldierModels", "Soldier4", new Model("s_m_y_cop_01"));
        soldierModel5 = settings.GetValue<Model>("SoldierModels", "Soldier5", new Model("s_m_y_cop_01"));
        // commanderModel = settings.GetValue<Model>("SoldierModels", "Commander", new Model("s_m_y_cop_01"));  // Uncomment if needed

        // Stop soldiers from speaking if they are spawned
        if (IsSpawned)
        {
            Function.Call(Hash.STOP_PED_SPEAKING, Soldier1, true);
            Function.Call(Hash.STOP_PED_SPEAKING, Soldier2, true);
            Function.Call(Hash.STOP_PED_SPEAKING, Soldier3, true);
            Function.Call(Hash.STOP_PED_SPEAKING, Soldier4, true);
            Function.Call(Hash.STOP_PED_SPEAKING, Soldier5, true);
        }

        // Handle post-firing behavior
        if (IsFired)
        {
            Script.Wait(500);  // Wait for half a second
            ClearSoldierTasks();  // Clear tasks for all soldiers
            Script.Wait(600);  // Wait for another 0.6 seconds
            Extension.FadeScreenOut(500);  // Fade out the screen
            Script.Wait(2500);  // Wait for 2.5 seconds
            MarkSoldiersAsNoLongerNeeded();  // Mark soldiers as no longer needed
            Sgt.MarkAsNoLongerNeeded();  // Mark the sergeant as no longer needed
            Prisoner1.IsPersistent = false;  // Make the prisoner non-persistent
            Prisoner1.IsPositionFrozen = false;  // Unfreeze the prisoner's position
            IsSpawned = false;  // Reset the spawned state
            IsFired = false;  // Reset the fired state
            IsAiming = false;  // Reset the aiming state
            Extension.FadeScreenIn(500);  // Fade the screen back in
        }
    }

    // Event handler for key down events
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == summonKey && !IsSpawned)
        {
            // Summon the firing squad if it's not already spawned
            Ped[] nearbyPeds = World.GetNearbyPeds(Game.Player.Character, 10f);
            if (nearbyPeds.Length == 0) return;

            Prisoner1 = nearbyPeds[0];
            Prisoner1.IsPersistent = true;
            Extension.FadeScreenOut(500);  // Fade out the screen
            Script.Wait(2500);  // Wait for 2.5 seconds

            // Spawn soldiers around the player
            Vector3 basePosition = Game.Player.Character.Position + Game.Player.Character.ForwardVector * 5f;

            Soldier1 = CreateSoldier(soldierModel1, basePosition, 0);
            Soldier2 = CreateSoldier(soldierModel2, Soldier1.Position + Soldier1.RightVector * 1f, 0);
            Soldier3 = CreateSoldier(soldierModel3, Soldier2.Position + Soldier2.RightVector * 1f, 0);
            Soldier4 = CreateSoldier(soldierModel4, Soldier1.Position + Soldier1.RightVector * -1f, 0);
            Soldier5 = CreateSoldier(soldierModel5, Soldier4.Position + Soldier4.RightVector * -1f, 0);

            Vector3 commanderPosition = Soldier5.Position + Soldier5.RightVector * -1f;
            Sgt = World.CreatePed("s_m_y_sheriff_01", commanderPosition);
            Sgt.Weapons.Give(WeaponHash.CarbineRifle, 210, true, true);

            Wait(200);  // Wait for 0.2 seconds

            Function.Call(Hash.SET_PED_PROP_INDEX, Sgt, 0, 0, 0, 0);  // Set sergeant's prop index

            Vector3 prisonerPosition = Soldier1.Position + Soldier1.ForwardVector * 5f;
            SetupPrisoner(prisonerPosition);

            Script.Wait(500);  // Wait for half a second
            IsSpawned = true;  // Set the spawned state to true
            Extension.FadeScreenIn(500);  // Fade the screen back in
        }
        else if (e.KeyCode == summonKey && IsSpawned && !IsFired)
        {
            // Dismiss the firing squad if it's already spawned and not fired
            DismissFiringSquad();
        }
        else if (e.KeyCode == fireKey && IsSpawned && !IsAiming)
        {
            // Command soldiers to aim at the prisoner
            CommandToAim();
        }
        else if (e.KeyCode == fireKey && IsSpawned && IsAiming)
        {
            // Command soldiers to fire at the prisoner
            CommandToFire();
        }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        // No specific actions needed on key up for now
    }

    // Method to create a soldier at a specific position with a specific model
    private Ped CreateSoldier(Model model, Vector3 position, float heading)
    {
        Ped soldier = World.CreatePed(model, position);
        soldier.Weapons.Give(WeaponHash.Musket, 100, true, true);
        soldier.Heading = heading;
        Script.Wait(5);  // Wait for 5 milliseconds
        return soldier;
    }

    // Method to make a soldier aim at the prisoner
    private void AimAtPrisoner(Ped soldier)
    {
        soldier.Task.AimAt(Prisoner1, -1);  // Aim at the prisoner indefinitely
        soldier.AlwaysKeepTask = true;  // Keep the task even if something else happens
    }

    // Method to set up the prisoner at a specific position
    private void SetupPrisoner(Vector3 position)
    {
        Prisoner1.Task.StandStill(9999999);  // Make the prisoner stand still indefinitely
        Prisoner1.AlwaysKeepTask = true;  // Keep the task even if something else happens
        Prisoner1.Position = position;  // Set the position of the prisoner
        Script.Wait(2500);  // Wait for 2.5 seconds
        Prisoner1.Heading = 180f;  // Set the heading of the prisoner
        Prisoner1.Task.HandsUp(9999999);  // Make the prisoner raise their hands indefinitely
        Prisoner1.IsPositionFrozen = true;  // Freeze the position of the prisoner
    }

    // Method to clear tasks for all soldiers
    private void ClearSoldierTasks()
    {
        Soldier1.Task.ClearAll();
        Soldier2.Task.ClearAll();
        Soldier3.Task.ClearAll();
        Soldier4.Task.ClearAll();
        Soldier5.Task.ClearAll();
    }

    // Method to mark soldiers as no longer needed
    private void MarkSoldiersAsNoLongerNeeded()
    {
        Soldier1.MarkAsNoLongerNeeded();
        Soldier2.MarkAsNoLongerNeeded();
        Soldier3.MarkAsNoLongerNeeded();
        Soldier4.MarkAsNoLongerNeeded();
        Soldier5.MarkAsNoLongerNeeded();
    }

    // Method to dismiss the firing squad
    private void DismissFiringSquad()
    {
        Extension.FadeScreenOut(500);  // Fade out the screen
        Script.Wait(2500);  // Wait for 2.5 seconds
        Soldier1.Delete();  // Delete the first soldier
        Soldier2.Delete();  // Delete the second soldier
        Soldier3.Delete();  // Delete the third soldier
        Soldier4.Delete();  // Delete the fourth soldier
        Soldier5.Delete();  // Delete the fifth soldier
        Sgt.Delete();  // Delete the sergeant
        Prisoner1.IsPositionFrozen = false;  // Unfreeze the prisoner's position
        Prisoner1.Task.ReactAndFlee(Game.Player.Character);  // Make the prisoner flee
        IsSpawned = false;  // Reset the spawned state
        IsFired = false;  // Reset the fired state
        IsAiming = false;  // Reset the aiming state
        Extension.FadeScreenIn(500);  // Fade the screen back in
    }

    // Method to command soldiers to aim at the prisoner
    private void CommandToAim()
    {
        Function.Call(Hash.PLAY_PED_AMBIENT_SPEECH_WITH_VOICE_NATIVE, Sgt, "SHOOTOUT_READY", "s_m_y_sheriff_01_white_full_01", "SPEECH_PARAMS_FORCE", 0);  // Play a speech line
        Script.Wait(2500);  // Wait for 2.5 seconds
        AimAtPrisoner(Soldier1);  // Make the first soldier aim at the prisoner
        AimAtPrisoner(Soldier2);  // Make the second soldier aim at the prisoner
        AimAtPrisoner(Soldier3);  // Make the third soldier aim at the prisoner
        AimAtPrisoner(Soldier4);  // Make the fourth soldier aim at the prisoner
        AimAtPrisoner(Soldier5);  // Make the fifth soldier aim at the prisoner
        IsAiming = true;  // Set the aiming state to true
    }

    // Method to command soldiers to fire at the prisoner
    private void CommandToFire()
    {
        Function.Call(Hash.PLAY_PED_AMBIENT_SPEECH_WITH_VOICE_NATIVE, Sgt, "SHOOTOUT_OPEN_FIRE", "s_m_y_sheriff_01_white_full_01", "SPEECH_PARAMS_FORCE", 0);  // Play a speech line
        Script.Wait(2000);  // Wait for 2 second
        Prisoner1.IsPositionFrozen = false;  // Unfreeze the prisoner's position
        Soldier1.Task.ShootAt(Prisoner1);  // Make the first soldier shoot at the prisoner
        Soldier2.Task.ShootAt(Prisoner1);  // Make the second soldier shoot at the prisoner
        Soldier3.Task.ShootAt(Prisoner1);  // Make the third soldier shoot at the prisoner
        Soldier4.Task.ShootAt(Prisoner1);  // Make the fourth soldier shoot at the prisoner
        Soldier5.Task.ShootAt(Prisoner1);  // Make the fifth soldier shoot at the prisoner
        IsFired = true;  // Set the fired state to true
    }
}
