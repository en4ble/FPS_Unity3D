using UnityEngine;
using System.Collections;

public class TeamMember : MonoBehaviour {

    int _teamID = 0;
    public int teamID
    {
        get { return _teamID; }
    }

    [RPC]
    void setTeamID(int id)
    {
        _teamID = id;

        this.transform.FindChild("soldier/swat_Military_Male_Lod_1").gameObject.SetActive(false);
        this.transform.FindChild("soldier/soldier_Military_Male_Lod_1").gameObject.SetActive(false);

        //set texture (+ color) for team
        if (_teamID == 0)
        {
            //vs everybody
            this.transform.FindChild("soldier/swat_Military_Male_Lod_1").gameObject.SetActive(false);
            this.transform.FindChild("soldier/soldier_Military_Male_Lod_1").gameObject.SetActive(true);

            SkinnedMeshRenderer mySkin = this.transform.GetComponentInChildren<SkinnedMeshRenderer>();
            //mark me red
            mySkin.material.color = new Color(0.8f, 0.2f, 0.2f);
        }
        else if (_teamID == 1)
        {
            //Terrorists
            this.transform.FindChild("soldier/swat_Military_Male_Lod_1").gameObject.SetActive(false);
            this.transform.FindChild("soldier/soldier_Military_Male_Lod_1").gameObject.SetActive(true);

            SkinnedMeshRenderer mySkin = this.transform.GetComponentInChildren<SkinnedMeshRenderer>();
            //mark me neutral
            mySkin.material.color = new Color(1f, 1f, 1f);
        }
        else if (_teamID == 2)
        {
            //Counterterrorists
            this.transform.FindChild("soldier/swat_Military_Male_Lod_1").gameObject.SetActive(true);
            this.transform.FindChild("soldier/soldier_Military_Male_Lod_1").gameObject.SetActive(false);

            SkinnedMeshRenderer mySkin = this.transform.GetComponentInChildren<SkinnedMeshRenderer>();
            //mark me neutral
            mySkin.material.color = new Color(1f, 1f, 1f);
        }
    }
}
