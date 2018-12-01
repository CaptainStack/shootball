using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Special Taco");
        if (collision.tag.Equals("Character"))
        {
            collision.GetComponent<Tds_Character>().EnableSpeedBoost();
            Destroy(this.gameObject);
        }
        else if (collision.tag.Equals("SpeedBoostSpawner"))
        {
            collision.GetComponent<SpeedBoostSpawner>().SuppressSpawn();
        }
    }
}
