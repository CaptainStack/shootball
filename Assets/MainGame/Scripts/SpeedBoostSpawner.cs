using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoostSpawner : MonoBehaviour {

    public float respawnFrequency;
    private float timeUntilNextSpawn;
    private SpeedBoost spawnedBoost;
    Vector3 startPos;

	// Use this for initialization
	void Start () {
        startPos = this.transform.position;
        timeUntilNextSpawn = respawnFrequency;
	}
	
	// Update is called once per frame
	void Update () {
        timeUntilNextSpawn -= Time.deltaTime;
        if (spawnedBoost != null)
        {
            timeUntilNextSpawn = respawnFrequency;
        }

        else if (timeUntilNextSpawn <= 0f)
        {
            timeUntilNextSpawn = respawnFrequency;
            SpawnSpeedBoost();
        }
	}

    private void SpawnSpeedBoost()
    {
        GameObject boost;
        boost = new GameObject("Speed Boost");
        CircleCollider2D collider = boost.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        boost.AddComponent<SpeedBoost>();
        boost.transform.position = this.transform.position;

        spawnedBoost = boost.GetComponent<SpeedBoost>();
    }

    public void SuppressSpawn()
    {
        Debug.Log("Suppress");
        timeUntilNextSpawn = respawnFrequency;
    }

}
