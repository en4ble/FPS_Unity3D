using UnityEngine;
using System.Collections;

public class Chat : MonoBehaviour {

    public bool chatActive = false;
    bool writeMessage = false;
    string message = "";
    bool team = false;  //false = message to everyone on the server, otherwise only your team will get the message

    NetworkManager nm;
    TeamMember tm;

	// Use this for initialization
	void Start () {
        nm = GameObject.FindObjectOfType<NetworkManager>();
	}
	
	// Update is called once per frame
	void Update () {
        if (writeMessage == true)
            nm.FreezeForChat();

        if(message != "" && !writeMessage)
        {
            tm = GameObject.FindObjectOfType<TeamMember>();

            if(team)
                nm.AddChatMessage(nm.GetPlayerName() + ": " +  message, tm.teamID);
            else
                nm.AddChatMessage(nm.GetPlayerName() + ": " + message, -1);     //-1 to message everyone on the server

            message = "";
        }
	}

    void OnGUI()
    {
        var isEnterPressed = (Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Return);
        if (isEnterPressed && chatActive)
        {
            if (writeMessage == true)
            {
                writeMessage = false;
                nm.UnFreeze();
            }
            else
            {
                writeMessage = true;
                nm.FreezeForChat();
            }
        }

        var isLShiftPressed = (Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Tab);
        if (isLShiftPressed && writeMessage)
        {
            tm = GameObject.FindObjectOfType<TeamMember>();

            if (team)
                team = false;
            else if (tm.teamID == 1 || tm.teamID == 2)
                team = true;
        }

        if(writeMessage)
        {
            GUI.BeginGroup(new Rect(Screen.width / 4, Screen.height / 8 * 7, Screen.width / 2, 50));

            if (!team)
                GUI.Label(new Rect(0, 0, 50, 50), "ALL");
            else
                GUI.Label(new Rect(0, 0, 50, 50), "TEAM");

            GUI.SetNextControlName("message");
            message = GUI.TextField(new Rect(50, 0, Screen.width / 2 - 50, 20), message.ToString());

            if (GUI.GetNameOfFocusedControl() == string.Empty)
                GUI.FocusControl("message");
                

            //end the group we started above
            GUI.EndGroup();
        }
    }
}
