using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityStandardAssets;


public class Player_NetworkSetup : NetworkBehaviour {


    [SerializeField]
    Camera fPSCharacterCam;

    [SerializeField]
    AudioListener audioListener;


    // Use this for initialization
    void Start ()
    {
	    //handling the control of the player so you do not control all players - each player is seperate
        if(isLocalPlayer)
        {
            //turn off main camera
            GameObject.Find("Scene Camera").SetActive(false);

            GetComponent<CharacterController>().enabled = true;
            //turn on the first person controller
            GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;

            //enable Camera on FirstPersonCharatcer - attached to player
            fPSCharacterCam.enabled = true;

            //enable Audio listener on the FirstPersonCharacter controller
            audioListener.enabled = true;

           
        }

	}
	
	
}
