using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    private AudioSource pickupAudio;

    // Use this for initialization
    void Start () 
    {
        pickupAudio = GetComponent<AudioSource> ();
        
    }
    
    // Update is called once per frame
    void Update () 
    {
        
    }

    private IEnumerator OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Character"))
        {
            // this.gameObject.SetActive(false);
            collision.GetComponent<Tds_Character>().EnableSpeedBoost();
            pickupAudio.Play();
            this.transform.position = new Vector3(-999, -999, -999);
            yield return new WaitForSeconds(1f);
            Destroy(this.gameObject);
        }
        else if (collision.tag.Equals("SpeedBoostSpawner"))
        {
            collision.GetComponent<SpeedBoostSpawner>().SuppressSpawn();
        }
    }
}
