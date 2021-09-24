using UnityEngine;
using System.Collections;
using UnityEditor;

public class TankMovement : MonoBehaviour {

    public float speed = 5;
    public float angularSpeed = 30;
    public float number = 1;                    //增加一个玩家的编号，通过编号区分不同的控制
    public AudioClip idleAudio;
    public AudioClip drivingAudio;
    public GameObject Turret;

    private AudioSource audio;
    private Rigidbody rigidbody1;
    private Rigidbody rigidbody2;

    // Use this for initialization
    void Start () {
        rigidbody1 = this.GetComponent<Rigidbody>();
        rigidbody2 = Turret.GetComponent<Rigidbody>();
        audio = this.GetComponent<AudioSource>();
	}

    void FixedUpdate() {
        float v = Input.GetAxis("VerticalPlayer"+number); //continuous
        rigidbody1.velocity = transform.forward*v*speed;

        float h = Input.GetAxis("HorizontalPlayer"+number);
        rigidbody1.angularVelocity = transform.up*h*angularSpeed;

        float t = Input.GetAxis("TurretPlayer" + number);
        rigidbody2.angularVelocity = transform.up * (t + h) * angularSpeed;

        if (Mathf.Abs(h) > 0.1 || Mathf.Abs(v) > 0.1) {
            audio.clip = drivingAudio;
            if(audio.isPlaying==false)
                audio.Play();
        }
        else {
            audio.clip = idleAudio;
            if (audio.isPlaying == false)
                audio.Play();
        }
    }
}
