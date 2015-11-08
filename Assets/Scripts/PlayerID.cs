using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerID : NetworkBehaviour {

    [SyncVar]
    private string playerUniqueIdentity;
    private NetworkInstanceId playerNetID;
    private Transform myTransform;


    public override void OnStartLocalPlayer()
    {
        //base.OnStartLocalPlayer();
        GetNetIdentity();
        SetIdentity();
    }


    // Use this for initialization
    void Awake () {

        myTransform = transform;

	}
	
	// Update is called once per frame
	void Update ()
    {
	    //update identity to other clients
        if(myTransform.name == "" || myTransform.name == "Player(Clone)")
        {
            SetIdentity();
        } 

	}


    [Client]
    //gets network ID from server then tells server what this players uniquieID is.
    void GetNetIdentity()
    {
        //returns a network id for this object
        playerNetID = GetComponent<NetworkIdentity>().netId;
        CmdTellServerMyIdentity(MakeUniqueIdentity());
    }

    void SetIdentity()
    {
        if(isLocalPlayer)
        {
            myTransform.name = playerUniqueIdentity;
        }
        else
        {
            myTransform.name = MakeUniqueIdentity();
        }
    }


    public string MakeUniqueIdentity()
    {
        string uniqueName = "Player" + playerNetID.ToString();
        return uniqueName;
    }

    //message from client to server
    [Command]
    void CmdTellServerMyIdentity(string name)
    {
        playerUniqueIdentity = name;
    }
}
