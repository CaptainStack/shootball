using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoostSpawner : MonoBehaviour {

    public float respawnFrequency;
    private float timeUntilNextSpawn;
    private SpeedBoost spawnedBoost;
    Vector3 startPos;

    public GameObject boost;

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
        Instantiate(boost, this.transform.position, Quaternion.identity);
    }

    public void SuppressSpawn()
    {
        Debug.Log("Suppress");
        timeUntilNextSpawn = respawnFrequency;
    }

}
