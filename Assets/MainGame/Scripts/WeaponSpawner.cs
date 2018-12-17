using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssemblyCSharp;

public class WeaponSpawner : MonoBehaviour 
{
    public float respawnFrequency;
    public Tds_GameManager vGameManager;
    private float timeUntilNextSpawn;
    private GameObject spawnedWeapon;
    Vector3 startPos;

    public GameObject akPrefab;
    public GameObject shotgunPrefab;
    public GameObject energyPrefab;
    // Use this for initialization
    void Start ()
    {
        startPos = this.transform.position;
        timeUntilNextSpawn = respawnFrequency;
    }
    
    // Update is called once per frame
    void Update ()
    {
        timeUntilNextSpawn -= Time.deltaTime;
        if (spawnedWeapon != null)
        {
            timeUntilNextSpawn = respawnFrequency;
        }
        else if (timeUntilNextSpawn <= 0f)
        {
            timeUntilNextSpawn = respawnFrequency;
            SpawnWeapon();
        }
    }

    private void SpawnWeapon()
    {
        int randomWeapon = Mathf.RoundToInt(Random.Range(0, 3));
        GameObject weaponPrefab;
        if (randomWeapon == 0) 
        {
            weaponPrefab = akPrefab;
        } 
        else if (randomWeapon == 1) 
        {
            weaponPrefab = shotgunPrefab;
        } else 
        {
            weaponPrefab = energyPrefab;
        }
        spawnedWeapon = Instantiate(weaponPrefab, this.transform.position, Quaternion.identity);
    }

    public void SuppressSpawn()
    {
        timeUntilNextSpawn = respawnFrequency;
    }
}
