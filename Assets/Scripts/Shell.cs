using UnityEngine;
using System.Collections;

public class Shell : MonoBehaviour 
{

    public GameObject shellExplosionPrefab;
    public AudioClip shellExplosionAudio;
    public void OnTriggerEnter(Collider collider)
    {
        //if (collider.gameObject.CompareTag("Tank2") && gameObject.transform.parent.name == "GreenTank1" || gameObject.transform.parent.name == "GreenTank2")
        //{
        //    Debug.Log("Name+tag:" + collider.gameObject.ToString()[0]);
        //}
        AudioSource.PlayClipAtPoint(shellExplosionAudio, transform.position);
        GameObject.Instantiate(shellExplosionPrefab, transform.position, transform.rotation);
        GameObject.Destroy(this.gameObject);
        
       
    }
}
