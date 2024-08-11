using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Step : MonoBehaviourPunCallbacks, IPunObservable
{
    Animator anim;
    [SerializeField]
    Button doorButton;
    bool isOpen = false;
    public PhotonView PV;
    string Activate;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (doorButton.GetisPush() && isOpen == false)
        {
            isOpen = true;
            StartCoroutine(StepOn());

        }
        else if (!doorButton.GetisPush() && isOpen == true)
        {
            isOpen = false;
            StartCoroutine(StepOn());
        }
    }


    IEnumerator StepOn()
    {
        yield return new WaitForSeconds(1.7f);
        PV.RPC("PlayDC", RpcTarget.AllBuffered, "Step");
        yield return new WaitForSeconds(10.0f);
        PV.RPC("PlayDC", RpcTarget.AllBuffered, "IdleStep");
    }

    [PunRPC]
    void PlayDC(string acivate)
    {
        Activate = acivate;
        anim.Play(Activate);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
