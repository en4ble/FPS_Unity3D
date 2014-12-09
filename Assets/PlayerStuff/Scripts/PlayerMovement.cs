using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

	// This component is only enabled for "my player" (i.e. the character belonging to the local client machine).
	// Remote players figures will be moved by a NetworkCharacter, which is also responsible for sending "my player's"
	// location to other computers.

	public float speed = 500f;	    // The speed at which I run
	public float jumpSpeed = 9f;	// How much power we put into our jump. Change this to jump higher
    public float skiingForce = 1f;
	public float jetforce = 15f;
    float rebound = 0.7f;
    float friction = 0.7f;
	public bool noBurnRate = false;
    public bool freeze = false;             //lock all
    public bool unlockCursor = false;

    float burnrate = 0.35F;
    float refillrate = 0.175F;
	float fuel = 1f;

    bool ski = false;
	bool jet = false;

    Vector3 onHit = Vector3.zero;
    Vector3 direction = Vector3.zero;	            // forward/back & left/right
	Vector3 currentSpeed = Vector3.zero;		    // speed aka v
	Vector3 currentAcceleration = Vector3.zero;	    // acceleration aka a

	SpawnSpot[] spawnSpots;

	CharacterController cc;
	Animator anim;

	// Use this for initialization
	void Start () {
		cc = GetComponent<CharacterController>();
		anim = GetComponent<Animator>();
		spawnSpots = GameObject.FindObjectsOfType<SpawnSpot>();
	}

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        spawnSpots = GameObject.FindObjectsOfType<SpawnSpot>();
    }

    void OnGUI()
    {
        //display fuel
        int barlength = (int)(12 + (Screen.height / 3) * Mathf.Clamp01(getFuel()));
        GUI.Box(new Rect(Screen.width - 60, Screen.height - 10 - barlength, 50, barlength), "FUEL");
    }

    public float getFuel()
    {
        return fuel;
    }

	bool fuelremains()
	{
		if (fuel > burnrate * Time.deltaTime)
			return true;
		return false;
	}

	// Update is called once per frame
	void Update () {
        if (!unlockCursor)
		    Screen.lockCursor = true;
        else
            Screen.lockCursor = false;

        if (!freeze)
        {
            jet = (Input.GetButton("Jet"));
            ski = (Input.GetButton("Ski"));
        }
        else
        {
            jet = false;
            ski = false;
        }

        if (!freeze || (!jet && Vector3.Angle(onHit, new Vector3(0, 1, 0)) > 50) && cc.isGrounded)
            // WASD forward/back & left/right movement is stored in "direction"
            direction = transform.rotation * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        else
            direction = Vector3.zero;

		// This ensures that we don't move faster going diagonally
		if (direction.magnitude > 1f) {
			direction = direction.normalized;
		}
		// Set our animation "Speed" parameter. This will move us from "idle" to "run" animations,
		// but we could also use this to blend between "walk" and "run" as well.

		anim.SetFloat("Speed", direction.magnitude);

		AdjustAimAngle();
	}

	void AdjustAimAngle() {
        if (!freeze)
        {
            Camera myCamera = this.GetComponentInChildren<Camera>();

            if (myCamera == null)
            {
                Debug.LogError("Why doesn't my character have a camera?  This is an FPS!");
                return;
            }

            float aimAngle = 0;

            if (myCamera.transform.rotation.eulerAngles.x <= 90f)
            {
                // We are looking DOWN
                aimAngle = -myCamera.transform.rotation.eulerAngles.x;
            }
            else
            {
                aimAngle = 360 - myCamera.transform.rotation.eulerAngles.x;
            }

            anim.SetFloat("AimAngle", aimAngle);
        }
	}

    void OnControllerColliderHit(ControllerColliderHit pCollision)
    {
        //onHit = orthogonal vector from collisionsurface
        //Debug.Log("WALL HIT");
        onHit = pCollision.normal;

        float x = 0;
        Vector3 cathetus = Vector3.zero;
        //vector needs to be divided into its 3 coordinates
        //  0 <= rebound | friction <= 1
        //  y = -y * rebound
        //  x = x * friction
        //  z = z * friction
        //maybe..
        //myAcceleration.y = |onHit| => |currentAcceleration|
        //myAcceleration.x = |<cathetusX>|
        //myAcceleration.z = |<cathetusZ>|
        if (!ski)
        {
            x = currentAcceleration.magnitude * onHit.magnitude;
            onHit *= x * rebound;
            //now normal vector and acceleration vector should have the same length
            cathetus = (onHit + currentAcceleration) * friction;
            currentAcceleration = onHit + cathetus;
        }

        //crashing against a object -> cc needs to be slowed down, stopped or even sliding down the object
        /*pseudo.code:
         * if(length(mySpeed, orthogonal to collisionsurface) < variable(e.g. 5))
         * {
         *      //slide down
         * }else{
         *      //bounce with speed /= .9 or sth like this
         *      Acceleration = Acceleration * onHit
         * }
        */
    }

    void OnCollisionExit(Collision pCollsion)
    {
        if(!Input.GetButton("Ski"))
            ski = false;
    }

	void FixedUpdate() {
		#region respawn
		if(spawnSpots == null) 
		{
			Debug.LogError ("no spawn spots");
		}else
		{
			if (cc.transform.position.y < -100)
			{
                PhotonNetwork.Destroy(gameObject);

                NetworkManager nm = GameObject.FindObjectOfType<NetworkManager>();

                nm.standbyCamera.SetActive(true);
                nm.respawnTimer = 5f;
			}
		}
		#endregion

        if (!fuelremains() && fuel < 1f || !jet && fuel < 1f)
            if (fuel + refillrate * Time.deltaTime < 1f)
                fuel += refillrate * Time.deltaTime;
            else
                fuel = 1f;  

		#region movement

        float upforcefactor = jetforce;

        if (Vector3.Angle(onHit, new Vector3(0, 1, 0)) > 50)
        {
            ski = true;
        }

        if (!cc.isGrounded)
        {
            anim.SetBool("Jumping", true);
            //air movement
            if (jet)
            {
                //use jetpack
                if (fuelremains())
                {
                    if (!noBurnRate)
                        fuel -= burnrate * Time.deltaTime;

                    currentAcceleration += direction * jetforce * Time.deltaTime;
                    currentAcceleration.y += upforcefactor * Time.deltaTime;
                }
            }

            // Apply gravity.
            currentAcceleration.y += Physics.gravity.y * Time.deltaTime;

            //air resistance
            currentAcceleration.x /= 1 + 0.0005F * Time.deltaTime * currentAcceleration.magnitude * currentAcceleration.magnitude; // quadratic drag
            currentAcceleration.z /= 1 + 0.0005F * Time.deltaTime * currentAcceleration.magnitude * currentAcceleration.magnitude; // quadratic drag
            if (currentAcceleration.y >= 0)
                currentAcceleration.y /= 1 + 0.0001F * Time.deltaTime * currentAcceleration.magnitude * currentAcceleration.magnitude; // less in y axis
            else
                currentAcceleration.y /= 1 + 0.00003F * Time.deltaTime * currentAcceleration.magnitude * currentAcceleration.magnitude; // less when down

            //if !jet -> no air control
            currentSpeed = currentAcceleration * Time.deltaTime;
        }
        else
        {
            anim.SetBool("Jumping", false);

            if (ski)
            {
                //skiing
                if (cc.isGrounded)
                {
                    //modify onHit for skiing
                    onHit.x *= skiingForce;
                    onHit.y = onHit.y - 1 * skiingForce;
                    onHit.z *= skiingForce;

                    //using modificated onHit for skiing
                    currentAcceleration.x += onHit.x;
                    currentAcceleration.z += onHit.z;
                }

                // factor 1000 is how big currentAcceleration.y can be (e.g. while skiing down a hill) - it is proportional to the tilt of the ground
                if ((currentAcceleration.y > onHit.y * 1000 && onHit.y > 0) || (currentAcceleration.y < onHit.y * 1000 && onHit.y < 0))
                    currentAcceleration.y = onHit.y * 1000;
                else
                    currentAcceleration.y += onHit.y * Time.deltaTime;

                currentSpeed = currentAcceleration * Time.deltaTime;
            }
            
            if(jet)
            {
                if (ski)
                    currentAcceleration.y = 0;

                //use jetpack
                if (fuelremains())
                {
                    if (!noBurnRate)
                        fuel -= burnrate * Time.deltaTime;
                }

                if (!ski)
                {
                    //so we can jump while skiing and keeping our acceleration AND we dont jump when we are skiing and using the jetpack
                    currentAcceleration = direction * speed * Time.deltaTime;
                    currentAcceleration.y = jumpSpeed;
                }

                anim.SetBool("Jumping", true);

                //apply jet
                currentSpeed = currentAcceleration * Time.deltaTime;
            }
            
            if(!jet)
            {
                //ground movement
                if (Input.GetButton("Jump"))
                {
                    if (!freeze)
                    {
                        //jump
                        if (!ski)
                            currentAcceleration = direction * speed * Time.deltaTime;

                        currentAcceleration.y = jumpSpeed;
                        currentSpeed = currentAcceleration * Time.deltaTime;
                        anim.SetBool("Jumping", true);
                    }
                }
                else if(!ski)
                {
                    if (currentAcceleration.y < 0)
                        anim.SetBool("Jumping", false);

                    direction.y = 0;
                    direction = direction.normalized;

                    currentAcceleration = direction * speed * Time.deltaTime;
                    currentSpeed = currentAcceleration * Time.deltaTime;
                    currentSpeed.y = Physics.gravity.y * Time.deltaTime;
                }
            }
        }

        cc.Move(currentSpeed);

        #endregion

        //Debug.Log("grounded = " + cc.isGrounded + " __ currentSpeed: " + currentSpeed + " __ currentAcceleration: " + currentAcceleration + " __ onHit: " + onHit);
        //Debug.Log("currentSpeed: " + currentSpeed + " __ currentAcceleration: " + currentAcceleration + " __ jet: " + jet + " __ fuel: " + fuel);
	}
}
