using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TankHealth : MonoBehaviour {

    public int hp = 100;
    public GameObject tankExplosion;
    public AudioClip tankExplosionAudio;
    public Slider hpSlider;

    private int hpTotal;
    private int hpDelta;
    private bool deadHp;

    // Use this for initialization
    void Start () {
	    hpTotal = hp;
        hpDelta = 0;
        deadHp = false;
    }

    public void TakeDamage() 
    {
        //hpDelta = Random.Range(10, 20);
        hpDelta = 50; // 这个可以改
        hp -= hpDelta;
        hpSlider.value = (float)hp /hpTotal;
        if (hp <= 0) {//收到伤害之后 血量为0 控制死亡效果
            deadHp = true;
            AudioSource.PlayClipAtPoint(tankExplosionAudio,transform.position);
            GameObject.Instantiate(tankExplosion, transform.position + Vector3.up, transform.rotation);
            ZeroHp();
            //ResetHp();
        }
    }
    public void ResetHp()
    {
        hp = 100;
        hpSlider.value = 1;
        deadHp = false;
    }
    public bool ZeroHp()
    {
         return deadHp;
    }
    public int Hp()
    {
        return hp;
    }
}
