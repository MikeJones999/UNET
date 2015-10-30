using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Player_SyncPosition : NetworkBehaviour {

    //variable gets synched across network - Server automatically transmit this value to all clients upon changing
    [SyncVar]
    private Vector3 syncPos;


    [SerializeField] Transform myTransform;
 
    
    [SerializeField] float lerpRate = 15;

    //Fixed upate to reduce the number of times information is sent over network
    void FixedUpdate()
    {
        TransmitPosition();
        LerpPosition();
    }


    //smoothing out position data of other players not local to us
    void LerpPosition()
    {
        if(!isLocalPlayer)
        {
            //my charachter will lerp between locations on other players computers
            myTransform.position = Vector3.Lerp(myTransform.position, syncPos, Time.deltaTime * lerpRate);
           
        }
    }

    //client tells server to run this command - require Cmd infront of method/function name
    [Command]
    void CmdProvidePositionToServer(Vector3 pos)
    {
        syncPos = pos;
    }

    //client only script - works on hosted servers(non dedicated) as well
    [ClientCallback]
    void TransmitPosition ()
    {
        //ensure isLocal player data being sent
        if(isLocalPlayer)
        CmdProvidePositionToServer(myTransform.position);
    }

}


