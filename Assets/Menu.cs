using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

    public bool showMenu = false;

    float sensitivity = 2f;
    bool invertX = false;
    bool invertY = false;
    int fov = 100;

    NetworkManager nm;

	// Use this for initialization
	void Start () {
        nm = GameObject.FindObjectOfType<NetworkManager>();
	}
	
	// Update is called once per frame
	void Update () {
        if (showMenu)
            nm.Freeze();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!showMenu)
            {
                showMenu = true;
                nm.Freeze();
            }
            else
            {
                showMenu = false;
                nm.UnFreeze();
            }
        }
	}

    void OnGUI()
    {
        if (showMenu)
        {
            GUI.BeginGroup(new Rect(Screen.width - 140, Screen.height / 2 - 150, 135, 202));
            // All rectangles are now adjusted to the group. (0,0) is the topleft corner of the group.

            // We'll make a box so you can see where the group is on-screen.
            GUI.Box(new Rect(0, 0, 135, 202), "Settings");

                GUI.BeginGroup(new Rect(10, 0, 130, 200));
                
                GUILayout.Label("");
                GUILayout.BeginHorizontal();
                GUILayout.Label("Field of View");
                fov = int.Parse(GUILayout.TextField(fov.ToString()));
                GUILayout.EndHorizontal();
                fov = (int)GUILayout.HorizontalSlider(fov, 90, 120);

                //mouse sensitivity
                GUILayout.Label("Mouse Sensitivity");
                string sensitivityString = GUILayout.TextField(sensitivity.ToString("0.00000"));
                float.TryParse(sensitivityString, out sensitivity);
                sensitivity = GUILayout.HorizontalSlider(sensitivity, 0.5f, 4f);

                //inverting?! 
                invertX = GUILayout.Toggle(invertX, " inverting x-axis");
                invertY = GUILayout.Toggle(invertY, " inverting y-axis");

                GUILayout.Button("credits");

                GUI.EndGroup();

            // End the group we started above. This is very important to remember!
            GUI.EndGroup();

            //set settings
            nm.ChangeSettings(sensitivity, invertX, invertY, fov);
        }
    }
}
