using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerPickupDrop : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody playerrigid;
    [SerializeField] private Animator anim;
    [SerializeField] private Text text;

    private float pickUpDistance = 0.7f;
    private Rigidbody CurrentObject;
    private bool isCarry = false;
    private bool isPush = false;
    public PhotonView PV;

    Vector3 height = new Vector3(0, 0.8f, 0);

    private void Start()
    {
        text.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (Physics.Raycast(player.position + height, player.forward, out RaycastHit ray, pickUpDistance))
        {
            if (ray.collider.tag == "Box" || ray.collider.tag == "Button")
            {
                if (PV.IsMine)
                {
                    text.enabled = true;
                }
            }
        }
        else
        {
                text.enabled = false;
        }

            if (isCarry)
        {
            gameObject.layer = 9;
        }
        else 
        {
            gameObject.layer = 10;
        }

        if (Input.GetKeyDown(KeyCode.E)) 
        {

            if (CurrentObject) 
            {
                CurrentObject.useGravity = true;
                CurrentObject.isKinematic = false;
                CurrentObject.transform.parent = null;
                CurrentObject = null;
                isCarry = false;
                return;
            }

            if (Physics.Raycast(player.position + height, player.forward, out RaycastHit raycastHit, pickUpDistance)) 
            {
                if (raycastHit.collider.tag == "Box")
                {
                    CurrentObject = raycastHit.rigidbody;
                    CurrentObject.transform.position = player.position;
                    CurrentObject.transform.forward = player.forward;
                    CurrentObject.useGravity = false;
                    CurrentObject.isKinematic = true;
                    CurrentObject.transform.parent = player.transform;
                    CurrentObject.transform.Translate(0, 2.8f, 0.6f);
                    isCarry = true;
                }

                if (raycastHit.collider.tag == "Button")
                {
                    CurrentObject = raycastHit.rigidbody;
                    player.position = CurrentObject.transform.position;
                    player.forward = CurrentObject.transform.forward;
                    player.transform.parent = CurrentObject.transform;
                    player.transform.Translate(0, 0, -1.4f);
                    isPush = true;
                    StartCoroutine(PushingButton());
                }
            }
        }      
    }

    IEnumerator PushingButton() 
    {
        anim.Play("Button Pushing");
        yield return new WaitForSeconds(1.7f);
        anim.Play("Blend Tree");
        player.transform.parent = null;
        CurrentObject = null;
        isPush = false;

    }

    public bool GetisCarrying() 
    {
        return isCarry;
    }

    public bool GetisPushing() 
    {
        return isPush;
    }
}
