using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

[NetworkSettings(channel = 0, sendInterval = 0.01f)]
public class Player_SyncPosition : NetworkBehaviour
{

    //variable gets synched across network - Server automatically transmit this value to all clients upon changing
    [SyncVar (hook = "SyncPositionValues")]
    private Vector3 syncPos;


    [SerializeField] Transform myTransform;
    [SerializeField] float lerpRate = 15;
 


    
    //this holds the players last position - used to update network only when player has moved 0.5 metres.
    private Vector3 lastPos;
    private float threshold = 0.4f;



    //handle some of the latency - create a list holds waypoints of player's movement - this list get sent to other clients who will use it to trace players movements
    //it provides a historic movemet of teh players location but the clienst dont know this and to them this is current
    private List<Vector3> syncPosList = new List<Vector3>();
    private float normalLerpRate = 18.0f;
    private float fasterLerpRate = 27.0f;
    [SerializeField] private bool useHistoricLerp = false;
    private float closeEnough = 0.11f; //0.1 metres

    void Start()
    {
        lerpRate = normalLerpRate;
    }
    

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
        if (!isLocalPlayer)
        {
            if(useHistoricLerp)
            {
                HistoricalLerping();
            }
            else
            {
                //call standard lerping method
                StandardLerping();
            }

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

    [Client]
    void SyncPositionValues(Vector3 latestPos)
    {
        syncPos = latestPos;
        syncPosList.Add(syncPos);
    }


    void StandardLerping()
    {
        if (!isLocalPlayer)
        {
            //my charachter will lerp between locations on other players computers
            myTransform.position = Vector3.Lerp(myTransform.position, syncPos, Time.deltaTime * lerpRate);
        }
    }


    //hold waypoints in list
    void HistoricalLerping()
    {
        //if empty - pointless doing anything
        if(syncPosList.Count > 0)
        {
            //interpolate towards the first position in the list
            myTransform.position = Vector3.Lerp(myTransform.position, syncPosList[0], Time.deltaTime * lerpRate);

            if(Vector3.Distance(myTransform.position, syncPosList[0]) < closeEnough)
            {
                //remove entry from the list as player has already reached there
                syncPosList.RemoveAt(0);
            }


            //player moved towards the waypoint a little quicker - inturn this removes an element from the array as teh waypoint is reached quicker
            if(syncPosList.Count > 10)
            {
                lerpRate = fasterLerpRate;
            }
            else
            {
                lerpRate = normalLerpRate;
            }

            Debug.Log("HistLerp " + syncPosList.Count.ToString());
        }
    }


}


