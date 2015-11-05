using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Player_SyncRotation : NetworkBehaviour
{

    //These variables will have their values sychronized from the server to clients in the game that are in the ready state. 
    //[SyncVar] marks it as dirty, so it will be sent to clients at the end of the current frame. Only simple values can be marked as [SyncVars].
    [SyncVar]
    private Quaternion syncPlayerRotation;
    [SyncVar]
    private Quaternion syncCamRotation;

    [SerializeField]
    private Transform playerTransform;
    [SerializeField]
    private Transform camTransform;
    [SerializeField]
    private float lerpRate = 15;

    //this holds the players last rotation - used to update network only when player has moved 5 degrees.
    private Quaternion lastPlayerRot;
    private Quaternion lastCamRot;
    private float threshold = 5.0f; //degrees

    void Update()
    {
        LerpRotations();
    }

    void FixedUpdate()
    {
        TransmitRotation();    
    }



    void LerpRotations()
    {

        if (!isLocalPlayer)
        {
            //Players rotation across network smoothly
            //from player rotation to syncplayerRotation - by time.deltatime
            //lerping - moving a percentage of the remaining distance of the target, which gets shorter and shorter as the movement and frame continue. 
            playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, syncPlayerRotation, Time.deltaTime * lerpRate);

            //cams rotation across network
            camTransform.rotation = Quaternion.Lerp(camTransform.rotation, syncCamRotation, Time.deltaTime * lerpRate);
        }
    }


    //client tells server to run this command - require Cmd infront of method/function name
    [Command]
    void CmdProvideRotationToServer(Quaternion playerRotate, Quaternion camRotate)
    {
        syncPlayerRotation = playerRotate;
        syncCamRotation = camRotate;
    }

    [Client]
    void TransmitRotation()
    {
        //ensure isLocal player data being sent & only when the rotation of the player or cam is 5 degrees since last rotation
        if (isLocalPlayer)
        {
            if (Quaternion.Angle(playerTransform.rotation, lastPlayerRot) > threshold || Quaternion.Angle(camTransform.rotation, lastCamRot) > threshold)
            {
                //provide my rotations to the server
                CmdProvideRotationToServer(playerTransform.rotation, camTransform.rotation);

                //update current rotations of player and cam
                lastPlayerRot = playerTransform.rotation;
                lastCamRot = camTransform.rotation;
            }
        }

    }
}