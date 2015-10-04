using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {
	public float speed = -1;
	private Transform spawnPoint;
	private Camera myCamera;

	void Start () {
		GetComponent<Rigidbody2D>().velocity = new Vector2(speed, 0);
		spawnPoint = GameObject.Find("SpawnPoint").transform;
		myCamera = Camera.main;
	
	}
	
	void OnBecameInvisible()
	{
		if (myCamera == null)
			return;
		float yMax = Camera.main.orthographicSize - 0.5f;
		transform.position = new Vector3( spawnPoint.position.x, 
		                                 Random.Range(-yMax, yMax), 
		                                 transform.position.z );
	}
}
