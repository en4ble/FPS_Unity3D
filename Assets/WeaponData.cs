using UnityEngine;
using System.Collections;

public class WeaponData : MonoBehaviour {
	public float fireRate = 0.5f;
	public float damage = 15f;
    public float reloadingTime = 4f;
    public bool shooting = true;
    public bool hitscan = true;
    public int magazineSize = 11;
    public int magazines = 5;
    float cooldownTimer;
    float reloadingTimer;
    int bullets;

    bool reloading = false;
    bool cooldown = false;

    public float GetCooldownTimer()
    {
        return cooldownTimer;
    }

    public float GetReloadingTimer()
    {
        return reloadingTimer;
    }

    public bool GetCooldown()
    {
        return cooldown;
    }

    public bool GetReloading()
    {
        return reloading;
    }
    
    public void Shoot()
    {
        if (shooting)
        {
            if (bullets > 0 && !cooldown)
            {
                bullets--;
                cooldown = true;
            }
            else if (bullets <= 0)
            {
                if (magazines > 0)
                {
                    reloading = true;
                    magazines--;
                }
                else
                    Debug.Log("no more magazines...");
            }
        }
        else
        {
            //knife
            reloading = true;
        }
    }

    public int GetBullets()
    {
        return bullets;
    }

	// Use this for initialization
	void Start () {
        bullets = magazineSize;
        reloadingTimer = reloadingTime;
        cooldownTimer = fireRate;
	}
	
	// Update is called once per frame
	void Update () {
        if (reloading)
        {
            reloadingTimer -= Time.deltaTime;
            if (reloadingTimer <= 0f)
            {
                bullets = magazineSize;
                reloadingTimer = reloadingTime;
                reloading = false;
            }
        }

        if(cooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = fireRate;
                cooldown = false;
            }
        }
	}
}
