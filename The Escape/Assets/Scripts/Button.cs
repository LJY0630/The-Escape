using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Button : MonoBehaviour
{
    BoxCollider HitBox;
    PlayerPickupDrop playerpickupdrop;
    bool isPush;
    bool isTouch;
    public AudioSource audioSource;
    public AudioClip press;

    // Start is called before the first frame update
    void Start()
    {
        HitBox = GetComponent<BoxCollider>();
        isPush = false;
        isTouch = false;
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerpickupdrop = other.gameObject.GetComponent<PlayerPickupDrop>();
            isTouch = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && playerpickupdrop != null)
        {
            if (playerpickupdrop.GetisPushing())
            {
                if (isPush == false && isTouch)
                {
                    isTouch = false;
                    isPush = true;
                    StartCoroutine(buttonSound());
                }
                else if (isPush == true && isTouch)
                {
                    isTouch = false;
                    isPush = false;
                    StartCoroutine(buttonSound());
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isTouch = false;
            playerpickupdrop = null;
        }
    }

    public bool GetisPush()
    {
        return isPush;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

    IEnumerator buttonSound()
    {
        audioSource.clip = press;
        audioSource.volume = 3.0f;
        yield return new WaitForSeconds(1.5f);
        audioSource.Play();
    }
}   
