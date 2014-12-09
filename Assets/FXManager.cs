using UnityEngine;
using System.Collections;

public class FXManager : MonoBehaviour {

	public GameObject sniperBulletFXPrefab;
    NetworkManager nm;
    AudioClip sniperBulletFXAudio;

    [RPC]
    void HitscanFX(Vector3 startPos, Vector3 endPos)
    {
        Vector3 v = Vector3.zero;
        Vector3 myPos = Vector3.zero;
        Vector3 bulletSound = Vector3.zero;
        Vector3 rayPos = Vector3.zero;
        float x = 0f;

        //visual
        Debug.Log("SniperBulletFX");

        if (sniperBulletFXPrefab != null)
        {
            GameObject sniperFX = (GameObject)Instantiate(sniperBulletFXPrefab, startPos, Quaternion.LookRotation(endPos - startPos));

            LineRenderer lr = sniperFX.transform.Find("LineFX").GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.SetPosition(0, startPos);
                lr.SetPosition(1, endPos);
            }
            else
            {
                Debug.LogError("sniperBulletFXPrefab's linerenderer is missing.");
            }

            //audio/shooting sound
            sniperBulletFXAudio = sniperBulletFXPrefab.audio.clip;
            nm = GameObject.FindObjectOfType<NetworkManager>();

            if (nm != null && sniperBulletFXAudio != null)
            {
                myPos = nm.GetMyPosition();
                v =  endPos - startPos;
                //get closest position between the other player and shooting ray
                x = (-(Vector3.Dot(startPos, v) -Vector3.Dot(myPos, v)))/(Mathf.Pow(v.x, 2) + Mathf.Pow(v.y, 2) + Mathf.Pow(v.z, 2));
                rayPos = startPos + x * v;

                //test what is the nearest to the enemy
                if (Vector3.Distance(startPos, myPos) < Vector3.Distance(endPos, myPos))
                {
                    if (Vector3.Distance(startPos, myPos) < Vector3.Distance(rayPos, myPos))
                        bulletSound = startPos;
                    else
                    {
                        if (x > 0)  //this ensures, that the shooting player is shooting in your direction
                            bulletSound = rayPos;
                        else
                            bulletSound = startPos;
                    }
                }
                else
                {
                    if (Vector3.Distance(endPos, myPos) < Vector3.Distance(rayPos, myPos) && x > 0)
                        bulletSound = endPos;
                    else
                    {
                        if (x > 0)  //this ensures, that the shooting player is shooting in your direction
                            bulletSound = rayPos;
                        else
                            bulletSound = endPos;
                    }
                }

                AudioSource.PlayClipAtPoint(sniperBulletFXAudio, bulletSound);
                audio.PlayOneShot(sniperBulletFXAudio);
            }
            else
                Debug.LogError("Something went wrong... NetworkManager Audio Shooting");
        }
        else
        {
            Debug.LogError("sniperBulletFXPrefab is missing!");
        }
    }


    [RPC]
    void KnifeFX(Vector3 startPos, Vector3 endPos)
    {
        GameObject sniperFX = (GameObject)Instantiate(sniperBulletFXPrefab, startPos, Quaternion.LookRotation(endPos - startPos));

        LineRenderer lr = sniperFX.transform.Find("LineFX").GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.SetPosition(0, startPos);
            lr.SetPosition(1, endPos);
        }
        else
        {
            Debug.LogError("sniperBulletFXPrefab's linerenderer is missing.");
        }
    }
}
