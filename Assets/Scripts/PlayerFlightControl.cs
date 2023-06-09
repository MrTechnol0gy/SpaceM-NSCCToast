﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

[System.Serializable]
public class PlayerFlightControl : MonoBehaviour
{
	public static PlayerFlightControl get; // Can also be used to set ;)

	[Header("References")]
	[Tooltip("Ship Renderer Tooltip!")]
	public Renderer shipRenderer;
	//"Objects", "For the main ship Game Object and weapons"));
	public GameObject actual_model; //"Ship GameObject", "Point this to the Game Object that actually contains the mesh for the ship. Generally, this is the first child of the empty container object this controller is placed in."
	public Transform weapon_hardpoint_1; //"Weapon Hardpoint", "Transform for the barrel of the weapon"
	public GameObject bullet; //"Projectile GameObject", "Projectile that will be fired from the weapon hardpoint."

	//"Core Movement", "Controls for the various speeds for different operations."
	public float speed; //"Base Speed", "Primary flight speed, without afterburners or brakes"
	public float afterburner_speed; //Afterburner Speed", "Speed when the button for positive thrust is being held down"
	public float slow_speed = 4f; //"Brake Speed", "Speed when the button for negative thrust is being held down"
	public float thrust_transition_speed = 5f; //"Thrust Transition Speed", "How quickly afterburners/brakes will reach their maximum effect"
	public float braking_transition_speed = 0.4f; //"Brake Transition Speed", "How quickly the ship will reduce speed to zero when thrust isn't active"
	public float turnspeed = 15.0f; //"Turn/Roll Speed", "How fast turns and rolls will be executed "
	public float rollSpeedModifier = 7; //"Roll Speed", "Multiplier for roll speed. Base roll is determined by turn speed"
	public float pitchYaw_strength = 0.5f; //"Pitch/Yaw Multiplier", "Controls the intensity of pitch and yaw inputs"

	//"Banking", "Visuals only--has no effect on actual movement"
	
	public bool use_banking = true; //Will bank during turns. Disable for first-person mode, otherwise should generally be kept on because it looks cool. Your call, though.
	public float bank_angle_clamp = 360; //"Bank Angle Clamp", "Maximum angle the spacecraft can rotate along the Z axis."
	public float bank_rotation_speed = 3f; //"Bank Rotation Speed", "Rotation speed along the Z axis when yaw is applied. Higher values will result in snappier banking."
	public float bank_rotation_multiplier = 1f; //"Bank Rotation Multiplier", "Bank amount along the Z axis when yaw is applied."
	
	public float screen_clamp = 500; //"Screen Clamp (Pixels)", "Once the pointer is more than this many pixels from the center, the input in that direction(s) will be treated as the maximum value."
	
	[HideInInspector]
	public float roll, yaw, pitch; //Inputs for roll, yaw, and pitch, taken from Unity's input system.	
	[HideInInspector]
	public bool slow_Active = false; //True if brakes are on
	
	float distFromVertical; //Distance in pixels from the vertical center of the screen.
	float distFromHorizontal; //Distance in pixel from the horizontal center of the screen.

	Vector2 mousePos = new Vector2(0,0); //Pointer position from CustomPointer
	
	float DZ = 0; //Deadzone, taken from CustomPointer.
	public float currentMag = 0f; //Current speed/magnitude
	
	bool thrust_exists = true;
	bool roll_exists = true;
	public bool PlayerIsBoinkedAbove = false;
	private float shootingInterval;
	private bool canShoot = true;

	public enum States
	{
		normal,			// normal = 0
		cloaked, 		// cloaked = 1
		afterburner 	// afterburner = 2
	}
	private States _currentState = States.normal;
	private float TimeStartedState;             // timer to know when we started a state
	
	private bool _tractorBeamActive = false;			// Whether the tractor beam is currently in use
	public bool tractorBeamActive
	{
		//When getting return the private variable _tractorBeamActive;
		get => _tractorBeamActive;
		set
		{
			//When setting, set _tractorBeamActive to the new value and then update the UI with the new information
            _tractorBeamActive = value;
			AudioManager.get.PlayTractorBeam();
		}
	}
	private bool _cloakActive = false;			// Whether the cloak is currently in use
	public bool cloakActive
	{
		//When getting return the private variable _cloakActive;
		get => _cloakActive;
		set
		{
			//When setting, set _cloakActive to the new value and then update the UI with the new information
			if (_cloakActive != value) {
				_cloakActive = value;
				AudioManager.get.PlayCloakActive();
				shipRenderer.DOKill();
				shipRenderer.material.DOColor(_cloakActive ? new Color(0.3f, 0.3f, 0.3f, 0.4f) : Color.white, 1)
					.SetEase(Ease.InOutQuad);
				//Debug.Log("Cloak is " + _cloakActive);
			}
			//UIManager.SetCloakActive(_cloakActive);
		}
	}
	private bool _afterburnerActive = false;			// Whether the afterburner is currently in use
	public bool afterburnerActive
	{
		//When getting return the private variable _afterburnerActive;
		get => _afterburnerActive;
		set
		{
			//When setting, set _afterburnerActive to the new value and then update the UI with the new information
            _afterburnerActive = value;
			Debug.Log("Afterburner is " + _afterburnerActive);
            //UIManager.SetAfterburnerActive(_afterburnerActive);
		}
	}
	
	//---------------------------------------------------------------------------------
	void Awake()
	{
		get = this;
	}
	void Start() 
	{	
		// gets relevant stats from the Player script
		speed = Player.get.maxSpeed;
		afterburner_speed = Player.get.afterburnerSpeed;	
		shootingInterval = Player.get.probeDelayBetweenShots;
		
		mousePos = new Vector2(0,0);	
		DZ = CustomPointer.instance.deadzone_radius;
		
		roll = 0; //Setting this equal to 0 here as a failsafe in case the roll axis is not set up.

		//Error handling, in case one of the inputs aren't set up.
		try {
			Input.GetAxis("Thrust");
		} catch {
			thrust_exists = false;
			Debug.LogError("(Flight Controls) Thrust input axis not set up! Go to Edit>Project Settings>Input to create a new axis called 'Thrust' so the ship can change speeds.");
		}
		
		try {
			Input.GetAxis("Roll");
		} catch {
			roll_exists = false;
			Debug.LogError("(Flight Controls) Roll input axis not set up! Go to Edit>Project Settings>Input to create a new axis called 'Roll' so the ship can roll.");
		}
		
		OnStartedState(currentState);
	}
	
	void FixedUpdate () {
		
		if (actual_model == null) {
			Debug.LogError("(FlightControls) Ship GameObject is null.");
			return;
		}
		
		
		updateCursorPosition();
		
		//Clamping the pitch and yaw values, and taking in the roll input.
		if (Player.get.allowPitch)
		{
			pitch = Mathf.Clamp(distFromVertical, -screen_clamp - DZ, screen_clamp  + DZ) * pitchYaw_strength;
		}
		else
		{
			pitch = 0;
		}
		//If player is boinked above ignore the real pitch and just set pitch to 45 degrees away from boinked object
		if (PlayerIsBoinkedAbove)
		{
			pitch = 45;
		}
		yaw = Mathf.Clamp(distFromHorizontal, -screen_clamp - DZ, screen_clamp  + DZ) * pitchYaw_strength;
		if (roll_exists)
			roll = (Input.GetAxis("Roll") * -rollSpeedModifier);
			
		
		//Getting the current speed.
		currentMag = GetComponent<Rigidbody>().velocity.magnitude;
		
		//If input on the thrust axis is positive, activate afterburners.

		if (thrust_exists) {
			if (Input.GetAxis("Thrust") > 0 && _afterburnerActive == false) 
			{
				slow_Active = false;
				currentMag = Mathf.Lerp(currentMag, speed, thrust_transition_speed * Time.deltaTime);
				Thruster.get.StartThrust();
			} 
			else if (Input.GetAxis("Thrust") > 0 && _afterburnerActive)
			{ 
				slow_Active = false;
				currentMag = Mathf.Lerp(currentMag, afterburner_speed, thrust_transition_speed * Time.deltaTime);
				Thruster.get.StartThrust();
			}
			else if (Input.GetAxis("Thrust") < 0) 
			{ 	//If input on the thrust axis is negatve, activate brakes.
				slow_Active = true;
				currentMag = Mathf.Lerp(currentMag, slow_speed, thrust_transition_speed * Time.deltaTime);
				Thruster.get.StopThrust();
			} 
			else
			{
				// if the player isn't holding thrust to move, they come to a stop gradually.
				currentMag = Mathf.Lerp(currentMag, 0, braking_transition_speed * Time.deltaTime);
				Thruster.get.StopThrust();
			}
			
		}
				
		//Apply all these values to the rigidbody on the container.
		GetComponent<Rigidbody>().AddRelativeTorque(
			(pitch * turnspeed * Time.deltaTime),
			(yaw * turnspeed * Time.deltaTime),
			(roll * turnspeed *  (rollSpeedModifier / 2) * Time.deltaTime));
		// If the Player script allows movement on the Y axis or not, respectively.
		Vector3 directionalVelocity;
		if (Player.get.allowPitch)
		{
			directionalVelocity = transform.forward;
		}
		else
		{
			directionalVelocity = new Vector3(transform.forward.x, 0, transform.forward.z);
		}
		// If the player is hit on the top of their ship, they will move at an angle away from the object
		if (PlayerIsBoinkedAbove)
		{
			directionalVelocity = Vector3.Lerp(directionalVelocity, -transform.up, 0.125f).normalized;
		}
		GetComponent<Rigidbody>().velocity = directionalVelocity * currentMag; //Apply speed
		
		if (use_banking)
		{
			updateBanking(); //Calculate banking.
		}
		if (transform.rotation.eulerAngles.x > 0f && transform.rotation.eulerAngles.x < 180f)
		{
			// Object is rotated downwards, so rotate it upwards towards zero.
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f), Time.deltaTime * bank_rotation_speed);
		}
		else if (transform.rotation.eulerAngles.x > 180f && transform.rotation.eulerAngles.x < 360f)
		{
			// Object is rotated upwards, so rotate it downwards towards zero.
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f), Time.deltaTime * bank_rotation_speed);
		}

		UIManager.SetSpeed(Mathf.RoundToInt(currentMag));	//convert speed from float to int to pass to the UIManager for display on the GUI		
	}		
		
		
	void updateCursorPosition() {

		mousePos = CustomPointer.pointerPosition;
		
		//Calculate distances from the center of the screen.
		float distV = Vector2.Distance(mousePos, new Vector2(mousePos.x, Screen.height / 2));
		float distH = Vector2.Distance(mousePos, new Vector2(Screen.width / 2, mousePos.y));
		
		//If the distances are less than the deadzone, then we want it to default to 0 so that no movements will occur.
		if (Mathf.Abs(distV) < DZ)
			distV = 0;
		else 
			distV -= DZ; 
			//Subtracting the deadzone from the distance. If we didn't do this, there would be a snap as it tries to go to from 0 to the end of the deadzone, resulting in jerky movement.
			
		if (Mathf.Abs(distH) < DZ)
			distH = 0;	
		else 
			distH -= DZ;
			
		//Clamping distances to the screen bounds.	
		distFromVertical = Mathf.Clamp(distV, 0, (Screen.height));
		distFromHorizontal = Mathf.Clamp(distH,	0, (Screen.width));	
	
		//If the mouse position is to the left, then we want the distance to go negative so it'll move left.
		if (mousePos.x < Screen.width / 2 && distFromHorizontal != 0) {
			distFromHorizontal *= -1;
		}
		//If the mouse position is above the center, then we want the distance to go negative so it'll move upwards.
		if (mousePos.y >= Screen.height / 2 && distFromVertical != 0) {
			distFromVertical *= -1;
		}
		

	}

	void updateBanking() 
	{
		if (Player.get.allowPitch)
		{
			//Load rotation information.
			Quaternion newRotation = transform.rotation;
			Vector3 newEulerAngles = newRotation.eulerAngles;
			
			//Basically, we're just making it bank a little in the direction that it's turning.
			newEulerAngles.z += Mathf.Clamp((-yaw * turnspeed * Time.deltaTime ) * bank_rotation_multiplier, - bank_angle_clamp, bank_angle_clamp);
			newRotation.eulerAngles = newEulerAngles;
			
			//Apply the rotation to the gameobject that contains the model.
			actual_model.transform.rotation = Quaternion.Slerp(actual_model.transform.rotation, newRotation, bank_rotation_speed * Time.deltaTime);
		}
		else
		{
			// Load rotation information.
			Quaternion newRotation = transform.rotation;
			Vector3 newEulerAngles = newRotation.eulerAngles;

			// Prevent rotation on X-axis.
			newEulerAngles.x = 0f;

			// Apply banking rotation on Z-axis.
			newEulerAngles.z += Mathf.Clamp((-yaw * turnspeed * Time.deltaTime) * bank_rotation_multiplier, -bank_angle_clamp, bank_angle_clamp);
			newRotation.eulerAngles = newEulerAngles;

			// Apply the rotation to the gameobject that contains the model.
			actual_model.transform.rotation = Quaternion.Slerp(actual_model.transform.rotation, newRotation, bank_rotation_speed * Time.deltaTime);
		}
	
	}
	
	void Update() 
	{	
		OnUpdatedState(currentState);
		//Please remove this and replace it with a shooting system that works for your game, if you need one.
		if (Input.GetMouseButtonDown(0) && canShoot) {
			StartCoroutine(fireShot());
			canShoot = false;
			AudioManager.get.PlayDistractionProbe();
		}
		//Checks if the player has collided with anything 'above' the ship and moves it away to prevent sticking
		PlayerIsBoinkedAbove = (Physics.Raycast(transform.position, transform.up, 1.5f, GameManager.get.environmentLayerMask, QueryTriggerInteraction.Ignore));
		// tractor beam controls
		if (Input.GetKeyDown(KeyCode.Space) && Player.get.pickupablesInRange.Count > 0)
		{
			tractorBeamActive = true;
		}		
		if (Input.GetKeyDown("left shift") && afterburnerActive == false)
		{
			afterburnerActive = true;
		}
		else if (Input.GetKeyDown("left shift") && afterburnerActive == true)
		{
			afterburnerActive = false;
		}
		if (Input.GetKeyDown(KeyCode.C) && cloakActive == false)
		{
			cloakActive = true;
		}
		else if (Input.GetKeyDown(KeyCode.C) && cloakActive == true)
		{
			cloakActive = false;
		}
	}
	
	
	public IEnumerator fireShot() 
	{
		if (weapon_hardpoint_1 == null) {
			Debug.LogError("(FlightControls) Trying to fire weapon, but no hardpoint set up!");
			yield return null;
		}
		
		if (bullet == null) {
			Debug.LogError("(FlightControls) Bullet GameObject is null!");
			yield return null;
		}
		
		//Shoots it in the direction that the pointer is pointing. Might want to take note of this line for when you upgrade the shooting system.
		if (Camera.main == null) {
			Debug.LogError("(FlightControls) Main camera is null! Make sure the flight camera has the tag of MainCamera!");
			yield return null;
		}
		
		GameObject shot1 = (GameObject) GameObject.Instantiate(bullet, weapon_hardpoint_1.position, Quaternion.identity);
		
		Ray vRay;
		
		if (!CustomPointer.instance.center_lock)
			vRay = Camera.main.ScreenPointToRay(CustomPointer.pointerPosition);
		else
			vRay = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
			
			
		RaycastHit hit;
		
		//If we make contact with something in the world, we'll make the shot actually go to that point.
		if (Physics.Raycast(vRay, out hit)) {
			shot1.transform.LookAt(hit.point);
			shot1.GetComponent<Rigidbody>().AddForce((shot1.transform.forward) * 9000f);
		
		//Otherwise, since the ray didn't hit anything, we're just going to guess and shoot the projectile in the general direction.
		} else {
			shot1.GetComponent<Rigidbody>().AddForce((vRay.direction) * 9000f);
		}
		yield return new WaitForSeconds(shootingInterval);
		canShoot = true;
	}
	public States currentState 
    {
        get => _currentState;
        set {
            if (_currentState != value) 
            {
                // Calling ended state for the previous state registered.
                OnEndedState(_currentState);
                
                // Setting the new current state
                _currentState = value;
                
                // Registering here the time we're starting the state
                TimeStartedState = Time.time;
                
                // Call the started state method for the new state.
                OnStartedState(_currentState);
            }
        }
    }
	// OnStartedState is for things that should happen when a state first begins
    public void OnStartedState(States state)
    {
        switch (state) 
        {
            case States.normal:
                //Debug.Log("I am normal.");
                break;
            case States.cloaked:
                //Debug.Log("I am cloaked.");
				if (afterburnerActive)
				{
					afterburnerActive = false;		// automatically turns the afterburner off
				}
				speed = speed / Player.get.cloakSpeed;
                break;
			case States.afterburner:
				//Debug.Log("I am using the afterburner.");
				break;
        }
    }
	// OnUpdatedState is for things that occur during the state (main actions)
    public void OnUpdatedState(States state) 
    {
        switch (state) 
        {
            case States.normal:
				if (cloakActive)
				{
					currentState = States.cloaked;
				}
                break;
            case States.cloaked:
				if (!cloakActive)
				{
					currentState = States.normal;
				}
				else if (afterburnerActive)
				{
					currentState = States.afterburner;
				}
                break;
			case States.afterburner:
				if (!afterburnerActive)
				{
					currentState = States.normal;
				}
				else if (cloakActive)
				{
					currentState = States.cloaked;
				}
				break;
        }
    }
    // OnEndedState is for things that should end or change when a state ends; for cleanup
    public void OnEndedState(States state)
    {
        switch (state) 
        {
            case States.normal:
                break;
            case States.cloaked:
			speed = Player.get.maxSpeed;
			if (cloakActive)
			{
				cloakActive = false;
			}
                break;
			case States.afterburner:
				break;
        }
    }

	public float GetSpeed()
	{
		return currentMag;
	}
}