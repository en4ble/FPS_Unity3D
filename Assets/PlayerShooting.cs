using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour {

    public bool freeze = false;

    float timerHitmarker = 0f;

	FXManager fxManager;
	WeaponData weaponData;
    GameObject hitmarker;

	void Start() {
		fxManager = GameObject.FindObjectOfType<FXManager>();

		if(fxManager == null) {
			Debug.LogError("Couldn't find an FXManager.");
		}

        hitmarker = GameObject.Find("Hitmarker");

        if (weaponData == null)
        {
            weaponData = gameObject.GetComponentInChildren<WeaponData>();
            if (weaponData == null)
            {
                Debug.LogError("Did not find any WeaponData in our children!");
                return;
            }
        }
	}

	// Update is called once per frame
	void Update () {
		if(Input.GetButton("Fire1") && !freeze)
        {
            //shoot
            Fire();
		}

        if (timerHitmarker > 0f)
        {
            //show hitmarker for timer intervall
            hitmarker.guiTexture.enabled = true;
            timerHitmarker -= Time.deltaTime;
        }
        else
            hitmarker.guiTexture.enabled = false;

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            //MG or spinfusor
            DisableWeapons();
            gameObject.transform.FindChild("soldier/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand/WeaponHolder/automatic").gameObject.SetActive(true);
            weaponData = gameObject.GetComponentInChildren<WeaponData>();
        }
        
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            //pistol or grenades
            DisableWeapons();
            gameObject.transform.FindChild("soldier/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand/WeaponHolder/pistol").gameObject.SetActive(true);
            weaponData = gameObject.GetComponentInChildren<WeaponData>();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            //knife
            DisableWeapons();
            gameObject.transform.FindChild("soldier/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand/WeaponHolder/knife").gameObject.SetActive(true);
            weaponData = gameObject.GetComponentInChildren<WeaponData>();
        }
	}

    void OnGUI()
    {
        //show mags
        if(weaponData.magazines > 0)
            GUI.Label(new Rect(50, Screen.height - 90, 100, 40), new GUIContent(weaponData.magazines + " Magazines"));
        else if (weaponData.magazines == 1)
            GUI.Label(new Rect(50, Screen.height - 90, 100, 40), new GUIContent(weaponData.magazines + " Magazine"));
        else
            GUI.Label(new Rect(50, Screen.height - 90, 100, 40), new GUIContent("-- Magazines"));

        
        if(!weaponData.GetReloading())
        {
            //show bullets
            if(weaponData.GetBullets() > 0)
                GUI.Label(new Rect(50, Screen.height - 70, 100, 20), new GUIContent(weaponData.GetBullets() + " Bullets"));
            else if (weaponData.GetBullets() == 1)
                GUI.Label(new Rect(50, Screen.height - 70, 100, 20), new GUIContent(weaponData.GetBullets() + " Bullet"));
            else
                GUI.Label(new Rect(50, Screen.height - 70, 100, 20), new GUIContent("-- Bullet"));
        }
        else
        {
            //show cooldown
            if(weaponData.magazines != -1)
                GUI.Label(new Rect(50, Screen.height - 70, 100, 20), new GUIContent("RELOADING " + weaponData.GetReloadingTimer() + " seconds"));
            else
                GUI.Label(new Rect(50, Screen.height - 70, 100, 20), new GUIContent("COOLDOWN " + weaponData.GetReloadingTimer() + " seconds"));
        }

    }

    public GameObject GetActiveWeapon()
    {
        Transform[] allChildren = gameObject.transform.FindChild("soldier/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand/WeaponHolder/").GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.tag == "Weapon")
            {
                return child.gameObject;
            }
        }

        return null;
    }


    void DisableWeapons()
    {
        Transform[] allChildren = gameObject.transform.FindChild("soldier/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand/WeaponHolder/").GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.tag == "Weapon")
            {
                child.transform.gameObject.SetActive(false);
                Debug.Log("disabled " + child.ToString());
            }
        }
    }

	void Fire() {
        if (weaponData == null)
        {
            Debug.LogError("Did not find any WeaponData in our children!");
            return;
        }

		if(weaponData.GetCooldown()) {
            Debug.Log("cooldown active..");
			return;
		}

        if (weaponData.GetReloading())
        {
            Debug.Log("on reload..");
            return;
        }

        if (weaponData.magazines != -1)
        {
            if (weaponData.magazines == 0 && weaponData.GetBullets() <= 0)
            {
                Debug.Log("no more magazines..");
                return;
            }
        }

        weaponData.Shoot();

		Debug.Log ("Firing!");

        Ray ray;
        Transform hitTransform;
        Vector3 hitPoint;

        if(weaponData.shooting == true)
        {
            //shooting
            ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

		    hitTransform = FindClosestHitObject(ray, out hitPoint);

		    if(hitTransform != null) {
			    Debug.Log ("We hit: " + hitTransform.name);

			    // We could do a special effect at the hit location
			    // DoRicochetEffectAt( hitPoint );

			    Health h = hitTransform.GetComponent<Health>();

			    while(h == null && hitTransform.parent) {
				    hitTransform = hitTransform.parent;
				    h = hitTransform.GetComponent<Health>();
			    }

			    // Once we reach here, hitTransform may not be the hitTransform we started with!

			    if(h != null) {
				    // This next line is the equivalent of calling:
				    //    				h.TakeDamage( damage );
				    // Except more "networky"
				    PhotonView pv = h.GetComponent<PhotonView>();
				    if(pv==null) {
					    Debug.LogError("Freak out!");
				    }
				    else {
                        TeamMember tm = hitTransform.GetComponent<TeamMember>();
                        TeamMember myTm = this.GetComponent<TeamMember>();

                        if(tm == null || tm.teamID == 0 || myTm == null || myTm.teamID == 0 || tm.teamID != myTm.teamID)
                        {
                            //We hit an enemy -> give him damage
                            h.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.AllBuffered, weaponData.damage);

                            //set timer to 0.1 sec
                            timerHitmarker = 0.1f;
                        }
				    }
			    }

			    if(fxManager != null) {
				    DoGunFX(hitPoint);
			    }
		    }
		    else {
			    // We didn't hit anything (except empty space), but let's do a visual FX (& Audio) anyway
			    if(fxManager != null) {
				    hitPoint = Camera.main.transform.position + (Camera.main.transform.forward*1000f);
				    DoGunFX(hitPoint);
			    }

		    }
        }
        else
        {
            //knifing - same, but other FX
            ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            hitTransform = FindClosestHitObject(ray, out hitPoint);

            if (hitTransform != null && Vector3.Distance(ray.origin, hitPoint) <= 2)
            {
                Debug.Log("We hit: " + hitTransform.name);

                // We could do a special effect at the hit location
                // DoRicochetEffectAt( hitPoint );

                Health h = hitTransform.GetComponent<Health>();

                while (h == null && hitTransform.parent)
                {
                    hitTransform = hitTransform.parent;
                    h = hitTransform.GetComponent<Health>();
                }

                // Once we reach here, hitTransform may not be the hitTransform we started with!

                if (h != null)
                {
                    // This next line is the equivalent of calling:
                    //    				h.TakeDamage( damage );
                    // Except more "networky"
                    PhotonView pv = h.GetComponent<PhotonView>();
                    if (pv == null)
                    {
                        Debug.LogError("Freak out!");
                    }
                    else
                    {
                        TeamMember tm = hitTransform.GetComponent<TeamMember>();
                        TeamMember myTm = this.GetComponent<TeamMember>();

                        if (tm == null || tm.teamID == 0 || myTm == null || myTm.teamID == 0 || tm.teamID != myTm.teamID)
                        {
                            //We hit an enemy -> give him damage
                            h.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.AllBuffered, weaponData.damage);

                            //set timer to 0.1 sec
                            timerHitmarker = 0.1f;
                        }
                    }

                }

                if (fxManager != null)
                {
                    DoKnifeFX(hitPoint);
                }
            }
            else
            {
                //didn't hit anything in our range
            }
        }
	}

	void DoGunFX(Vector3 hitPoint) {
		fxManager.GetComponent<PhotonView>().RPC("HitscanFX", PhotonTargets.All, weaponData.transform.position, hitPoint);
	}

    void DoKnifeFX(Vector3 hitPoint)
    {
        fxManager.GetComponent<PhotonView>().RPC("KnifeFX", PhotonTargets.All, weaponData.transform.position, hitPoint);
    }


	Transform FindClosestHitObject(Ray ray, out Vector3 hitPoint) {

		RaycastHit[] hits = Physics.RaycastAll(ray);

		Transform closestHit = null;
		float distance = 0;
		hitPoint = Vector3.zero;

		foreach(RaycastHit hit in hits) {
			if(hit.transform != this.transform && ( closestHit==null || hit.distance < distance ) ) {
				// We have hit something that is:
				// a) not us
				// b) the first thing we hit (that is not us)
				// c) or, if not b, is at least closer than the previous closest thing

				closestHit = hit.transform;
				distance = hit.distance;
				hitPoint = hit.point;
			}
		}

		// closestHit is now either still null (i.e. we hit nothing) OR it contains the closest thing that is a valid thing to hit

		return closestHit;

	}
}
