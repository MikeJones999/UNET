using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Player_SyncRotation : NetworkBehaviour {

    [SyncVar]    private Quaternion syncPlayerRotation;
    [SyncVar]    private Quaternion syncCamRotation;

    [SerializeField]    private Transform playerTransform;
    [SerializeField]    private Transform camTransform;
    [SerializeField]    private float lerpRate = 15;


    void LerpRotations()
    {

    }

	
}
