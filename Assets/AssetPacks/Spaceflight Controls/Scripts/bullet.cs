using UnityEngine;
using System.Collections;

public class bullet : MonoBehaviour {

	public GameObject explo;
	

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	
	}
	
	
	void OnCollisionEnter(Collision col) 
	{
		// Check if the bullet has hit the player
		if (col.gameObject.tag == "Player") 
		{
			GameTimer.get.DecreaseTime(5);
			//Debug.Log("Bullet hit the player!");
		} 
		else 
		{
			// Code to handle hitting other objects goes here
			GameObject.Instantiate(explo, col.contacts[0].point, Quaternion.identity);
		}
		Destroy(gameObject);
	}
}
