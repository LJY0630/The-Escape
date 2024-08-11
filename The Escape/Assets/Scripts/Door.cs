using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Door : MonoBehaviourPunCallbacks, IPunObservable
{
    Animator anim;
    [SerializeField]
    Button doorButton;
    bool isOpen = false;
    public PhotonView PV;
    string Activate;
    AudioSource audioSource;
    public AudioClip doorSliding;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (doorButton.GetisPush() && isOpen == false) 
        {
            isOpen = true;
            StartCoroutine(DoorOpen());

        }
        else if (!doorButton.GetisPush() && isOpen == true)
        {
            isOpen = false;
            StartCoroutine(DoorClose());
        }
    }


    IEnumerator DoorOpen()
    {
        yield return new WaitForSeconds(1.7f);
        PV.RPC("PlayDC", RpcTarget.AllBuffered, "DoorOpen");
        slidingS();
    }

    IEnumerator DoorClose()
    {
        yield return new WaitForSeconds(1.7f);
        PV.RPC("PlayDC", RpcTarget.AllBuffered, "DoorClose");
        slidingS();
    }

    [PunRPC]
    void PlayDC(string acivate)
    {
        Activate = acivate;
        anim.Play(Activate);
    }

    void slidingS()
    {
        audioSource.clip = doorSliding;
        audioSource.volume = 8.0f;
        audioSource.pitch = 1.0f;
        audioSource.Play();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
