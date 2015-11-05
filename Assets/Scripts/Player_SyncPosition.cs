using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[NetworkSettings(channel = 0, sendInterval = 0.033f)]
public class Player_SyncPosition : NetworkBehaviour
{

    //variable gets synched across network - Server automatically transmit this value to all clients upon changing
    [SyncVar]
    private Vector3 syncPos;


    [SerializeField] Transform myTransform;    
    [SerializeField] float lerpRate = 15;

    //this holds the players last position - used to update network only when player has moved 0.5 metres.
    private Vector3 lastPos;
    private float threshold = 0.4f;


    void Update()
    {
        //lerping need to be called from here otherwise in fixedupdate it remains the same which causes slow movement
        //on sime an fast on others
        LerpPosition();
    }


    //Fixed upate to reduce the number of times information is sent over network
    void FixedUpdate()
    {
        TransmitPosition();
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
        Debug.Log("Command Called - synchPosition");
    }

    //client only script - works on hosted servers(non dedicated) as well
    [ClientCallback]
    void TransmitPosition ()
    {
        //ensure isLocal player data being sent & only when the distance of the player is 0.5 metres since lats movement
        if (isLocalPlayer && Vector3.Distance(myTransform.position, lastPos) >= threshold)
        {
            CmdProvidePositionToServer(myTransform.position);

            //update lastpos with my newest current position
            lastPos = myTransform.position;
        }
    }

}


