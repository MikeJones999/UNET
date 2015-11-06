using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Player_SyncRotation : NetworkBehaviour
{

    //These variables will have their values sychronized from the server to clients in the game that are in the ready state. 
    //[SyncVar] marks it as dirty, so it will be sent to clients at the end of the current frame. Only simple values can be marked as [SyncVars].
    [SyncVar (hook = "OnPlayerRotSynched")]
    private float syncPlayerRotation;
    [SyncVar (hook = "OnCamRotSynched")]
    private float syncCamRotation;

    [SerializeField]
    private Transform playerTransform;
    [SerializeField]
    private Transform camTransform;
   
    private float lerpRate = 15;

    //this holds the players last rotation - used to update network only when player has moved 5 degrees.
    //only sending floats - say x of player and y of cam as this saves bandwidth - no need to send quaternions as wholes
    private float lastPlayerRot;
    private float lastCamRot;
    private float threshold = 1.0f; //degrees


    //historic rotation for smoother process and appearance
    private List<float> syncPlayPosList = new List<float>();
    private List<float> syncCamPosList = new List<float>();
    private float closeEnough = 0.5f;
    [SerializeField]
    private bool useHistoricalInterpolation;
     

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
            //Old method when using quaternions
                //Players rotation across network smoothly
                //from player rotation to syncplayerRotation - by time.deltatime
                //lerping - moving a percentage of the remaining distance of the target, which gets shorter and shorter as the movement and frame continue. 
             //playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, syncPlayerRotation, Time.deltaTime * lerpRate);

                    //cams rotation across network
              // camTransform.rotation = Quaternion.Lerp(camTransform.rotation, syncCamRotation, Time.deltaTime * lerpRate);
            //Old method when using quaternions

            //new method
            if(useHistoricalInterpolation)
            {

            }
            else
            {
                OrdinaryLerping();
            }


        }
    }



    void OrdinaryLerping()
    {
        LerpPlayerRotations(syncPlayerRotation);
        LerpCamRotations(syncCamRotation);
    }

    //create a vector 3 to represent the player's rotation. That will be used in conjunction iwth quaternion to lerp player
    void LerpPlayerRotations(float playRot)
    {
        Vector3 playerNewRot = new Vector3(0, playRot, 0);
        playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, Quaternion.Euler(playerNewRot), lerpRate * Time.deltaTime);
    }

    void LerpCamRotations(float camRot)
    {
        Vector3 camNewRot = new Vector3(camRot, 0, 0);
        //rotatate relevate to the parent
        camTransform.localRotation = Quaternion.Lerp(camTransform.localRotation, Quaternion.Euler(camNewRot), lerpRate * Time.deltaTime);
    }

    //client tells server to run this command - require Cmd infront of method/function name
    [Command]
    //void CmdProvideRotationToServer(Quaternion playerRotate, Quaternion camRotate)
    void CmdProvideRotationToServer(float playerRotate, float camRotate)
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
            //if (Quaternion.Angle(playerTransform.rotation, lastPlayerRot) > threshold || Quaternion.Angle(camTransform.rotation, lastCamRot) > threshold)
            if(CheckIfBeyondThreshold(playerTransform.localEulerAngles.y, lastPlayerRot) || CheckIfBeyondThreshold(camTransform.localEulerAngles.x, lastCamRot))
            {
                //provide my rotations to the server - quaternions
                //CmdProvideRotationToServer(playerTransform.rotation, camTransform.rotation);

                //update current rotations of player and cam - quaternions
                //lastPlayerRot = playerTransform.rotation;
                //lastCamRot = camTransform.rotation;

                lastPlayerRot = playerTransform.localEulerAngles.y;
                lastCamRot = camTransform.localEulerAngles.x;
                //provide my rotations to the server
                CmdProvideRotationToServer(lastPlayerRot, lastCamRot);
            }
        }

    }

    bool CheckIfBeyondThreshold(float rot1, float rot2)
    {
        if (Mathf.Abs(rot1 - rot2) > threshold)
        {
            return true;
        }
        else return false;
    }


    //get the latest player rotation and add to list
    [Client]
    void OnPlayerRotSynched(float latestPlayerRot)
    {
        syncPlayerRotation = latestPlayerRot;
        syncPlayPosList.Add(syncPlayerRotation);
    }

    //get the latest cam rotation and add to list
    [Client]
    void OnCamRotSynched(float latestCamRot)
    {
        syncCamRotation = latestCamRot;
        syncCamPosList.Add(syncCamRotation);
    }



}