﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieController : MonoBehaviour {
	[SerializeField]
	private PolygonCollider2D[] colliders;
	private int currentColliderIndex = 0;

	public float moveSpeed;
	public float turnSpeed;
	private Vector3 moveDirection = Vector3.right;
	private List<Transform> congaLine = new List<Transform>();

	private bool isInvincible = false;
	private float timeSpentInvincible;
	private int lives = 3;
	public AudioClip enemyContactSound;
	public AudioClip enemyContactScreamSound;
	public AudioClip catContactSound;

	// Update is called once per frame
	void Update () {
		// 1
		Vector3 currentPosition = transform.position;
		// 2
		if( Input.GetButton("Fire1") ) {
			// 3
			Vector3 moveToward = Camera.main.ScreenToWorldPoint( Input.mousePosition );
			// 4
			moveDirection = moveToward - currentPosition;
			moveDirection.z = 0; 
			moveDirection.Normalize();
		}

		Vector3 target = moveDirection * moveSpeed + currentPosition;
		transform.position = Vector3.Lerp( currentPosition, target, Time.deltaTime );

		float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
		transform.rotation = 
			Quaternion.Slerp( transform.rotation, 
			                 Quaternion.Euler( 0, 0, targetAngle ), 
			                 turnSpeed * Time.deltaTime );
	
		EnforceBounds();

		//1
		if (isInvincible)
		{
			//2
			timeSpentInvincible += Time.deltaTime;
			
			//3
			if (timeSpentInvincible < 3f) {
				float remainder = timeSpentInvincible % .3f;
				GetComponent<Renderer>().enabled = remainder > .15f; 
			}
			//4
			else {
				GetComponent<Renderer>().enabled = true;
				isInvincible = false;
			}
		}

	}

	public void SetColliderForSprite( int spriteNum )
	{
		colliders[currentColliderIndex].enabled = false;
		currentColliderIndex = spriteNum;
		colliders[currentColliderIndex].enabled = true;
	}

	void OnTriggerEnter2D( Collider2D other )
	{
		if(other.CompareTag("cat")) {
			Transform followTarget = congaLine.Count == 0 ? transform : congaLine[congaLine.Count-1];
			other.transform.parent.GetComponent<CatController>().JoinConga( followTarget, moveSpeed, turnSpeed );
			congaLine.Add( other.transform );
			GetComponent<AudioSource>().PlayOneShot(catContactSound);

			if (congaLine.Count >= 5) {
				Debug.Log("You won!");
				Application.LoadLevel("WinScene");
			}
		}
		else if(!isInvincible && other.CompareTag("enemy")) {
			isInvincible = true;
			timeSpentInvincible = 0;
			GetComponent<AudioSource>().PlayOneShot(enemyContactSound);
			GetComponent<AudioSource>().PlayOneShot(enemyContactScreamSound);

			if (--lives <= 0) {
				Debug.Log("You lost!");
				Application.LoadLevel("LoseScene");
			}

			for( int i = 0; i < 2 && congaLine.Count > 0; i++ )
			{
				int lastIdx = congaLine.Count-1;
				Transform cat = congaLine[ lastIdx ];
				congaLine.RemoveAt(lastIdx);
				cat.parent.GetComponent<CatController>().ExitConga();
			}
		}
	}

	private void EnforceBounds()
	{
		// 1
		Vector3 newPosition = transform.position; 
		Camera mainCamera = Camera.main;
		Vector3 cameraPosition = mainCamera.transform.position;
		
		// 2
		float xDist = mainCamera.aspect * mainCamera.orthographicSize; 
		float xMax = cameraPosition.x + xDist;
		float xMin = cameraPosition.x - xDist;

		float yDist = mainCamera.orthographicSize; 
		float yMax = cameraPosition.y + yDist;
		float yMin = cameraPosition.y - yDist;
		
		// 3
		if ( newPosition.x < xMin || newPosition.x > xMax ) {
			newPosition.x = Mathf.Clamp( newPosition.x, xMin, xMax );
			moveDirection.x = -moveDirection.x;
		}

		if ( newPosition.y < yMin || newPosition.y > yMax ) {
			newPosition.y = Mathf.Clamp( newPosition.y, yMin, yMax );
			moveDirection.y = -moveDirection.y;
		}

		// 4
		transform.position = newPosition;
	}
}
