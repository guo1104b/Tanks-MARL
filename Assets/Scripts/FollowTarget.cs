using UnityEngine;
using System.Collections;

public class FollowTarget : MonoBehaviour {

    public Transform player1;
    public Transform player2;

    private Vector3 offset;
    private Camera camera;

	// Use this for initialization
	void Start () {
	    camera = this.GetComponent<Camera>();
		offset = camera.transform.position - (player1.position + player2.position) / 2;
	}
	
	// Update is called once per frame
	void Update () {
	    if (player1 == null || player2 == null) return;
	    transform.position = (player1.position + player2.position)/2 + offset;
	    float distance = Vector3.Distance(player1.position, player2.position);
	    float size = distance*0.58f;
	    camera.orthographicSize = size;
	}
}
