using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Player_SyncRotation : NetworkBehaviour
{

    //These variables will have their values sychronized from the server to clients in the game that are in the ready state. 
    //[SyncVar] marks it as dirty, so it will be sent to clients at the end of the current frame. Only simple values can be marked as [SyncVars].
    [SyncVar]    private Quaternion syncPlayerRotation;
    [SyncVar]    private Quaternion syncCamRotation;

    [SerializeField]   private Transform playerTransform;
    [SerializeField]    private Transform camTransform;
    [SerializeField]    private float lerpRate = 15;

    void FixedUpdate()
    {
        TransmitRotation();
        LerpRotations();
    }



    void LerpRotations()
    {

        if (!isLocalPlayer)
        {
            //Players rotation across network smoothly
            //from player rotation to syncplayerRotation - by time.deltatime
            playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, syncPlayerRotation, Time.deltaTime * lerpRate);

            //cams rotation across network
            camTransform.rotation = Quaternion.Lerp(camTransform.rotation, syncCamRotation, Time.deltaTime * lerpRate);
        }
    }

    [Command]
    void CmdProvideRotationToServer(Quaternion playerRotate, Quaternion camRotate)
    {
        syncPlayerRotation = playerRotate;
        syncCamRotation = camRotate;
    }

    [Client]
    void TransmitRotation()
    {
        if(isLocalPlayer)
        {
            //provide my rotations to the server
            CmdProvideRotationToServer(playerTransform.rotation, camTransform.rotation);
        }
    }

}