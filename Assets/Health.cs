using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {

	public float hitPoints = 100f;
    float timerOnHit = 0f;
    float timerOnHitDisappear = 0f;
    GameObject onHit;

	void Start () {
        onHit = GameObject.Find("OnHit");
	}
	
	[RPC]
	public void TakeDamage(float amt) {
        hitPoints -= amt;

        if (hitPoints < 0)
			Die();

        if (GetComponent<PhotonView>().isMine)
        {
            if (amt <= 5)
                timerOnHit += 1f;
            else
                timerOnHit += amt / 5;
        }
	}

    void Update()
    {
        if (GetComponent<PhotonView>().isMine && gameObject.tag == "Player")
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                Die();
            }
        }

        if (timerOnHit > 0f)
        {
            //modify GUI image onHit alpha color
            onHit.guiTexture.color = new Color(0.3f, 0.3f, 0.3f, timerOnHit / 10);

            //show onHit for timer intervall
            onHit.guiTexture.enabled = true;
            timerOnHit -= Time.deltaTime;
        }
        else
            timerOnHitDisappear = 5f;

        if (timerOnHitDisappear > 0f && timerOnHit < 0f)
        {
            onHit.guiTexture.color = new Color(0.3f, 0.3f, 0.3f, onHit.guiTexture.color.a - (1f - (float)(timerOnHitDisappear / 5)));
            timerOnHitDisappear -= Time.deltaTime;
        }
        else if(timerOnHitDisappear < 0f && timerOnHit < 0f)
            onHit.guiTexture.enabled = false;
    }

    void OnGUI()
    {
        if (GetComponent<PhotonView>().isMine)
        {
            //modify GUI color for colored hitPoints
            GUI.color = new Color((100 - hitPoints) / 100, hitPoints / 100, 0f);
            GUI.Label(new Rect(Screen.width - 120, Screen.height - 50, 50, 40), new GUIContent(hitPoints + " HP"));
        }
    }

	void Die() {
        if (GetComponent<PhotonView>().instantiationId == 0)
            Destroy(gameObject);
        else
        {
            if(GetComponent<PhotonView>().isMine)
            {
                if(gameObject.tag == "Player")
                {
                    NetworkManager nm = GameObject.FindObjectOfType<NetworkManager>();

                    nm.standbyCamera.SetActive(true);
                    nm.respawnTimer = 5f;
                }

                PhotonNetwork.Destroy(gameObject);
            }
        }
	}
}
