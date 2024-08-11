using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    Animator anim;

    [SerializeField]
    private Transform playerbody;

    [SerializeField]
    private Transform cameraArm;

    [SerializeField]
    private PlayerPickupDrop PickupDrop;

    [SerializeField]
    private GameObject GPanel;

    [SerializeField]
    private GameObject CPanel;

    public AudioClip walk;
    public AudioClip die;
    public float speed;
    public float runSpeed = 8f;
    public bool run;
    public float smoothness = 10f;
    public float jumpForce;
    AudioSource audioSource;

    private float finalSpeed;
    private float curSpeed;
    private Vector2 moveInput = Vector2.zero;
    private Rigidbody rigid;
    private bool jumpDown = false;
    private bool jumping = false;
    private bool hitWall = false;
    private bool isDead = false;
    private Vector3 startVector;
    private Vector3 curvelocity;
    public PhotonView PV;
    public Text MasterName, OtherName;
    public string mine;
    public string other;
    Vector3 moveDirection;
    public Vector3 curPos;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        GPanel.SetActive(false);
        CPanel.SetActive(false);
        startVector = transform.position;
        mine = PhotonNetwork.NickName;
        Debug.Log(mine);
        if (PhotonNetwork.IsMasterClient)
        {
            audioSource = GameObject.Find("Futuristic_G").GetComponent<AudioSource>();
        }
        else
        {
            audioSource = GameObject.Find("Futuristic_Red").GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        PV.RPC("Name", RpcTarget.Others, mine);

        if (PV.IsMine)
        {
            if (!isDead)
            {
                moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

                if (Input.GetButtonDown("Jump") && !PickupDrop.GetisCarrying() && !PickupDrop.GetisPushing())
                {
                    jumpDown = true;
                    Jump();
                }

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    run = true;
                }
                else
                {
                    run = false;
                }

                if (Input.GetKey(KeyCode.P))
                {
                    Die();
                }
            }
        }
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!PickupDrop.GetisPushing() && PV.IsMine && !isDead)
        {
            InputMovement();
        }
    }

    void InputMovement()
    {
        if (moveInput != Vector2.zero && !PickupDrop.GetisPushing())
        {
            Vector3 forward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
            Vector3 right = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
            moveDirection = curvelocity = forward * moveInput.y + right * moveInput.x;

            if (!jumpDown)
            {
                finalSpeed = (run) ? runSpeed : speed;
            }

            if (hitWall && jumping)
            {
                finalSpeed = 0;
            }

            PV.RPC("front", RpcTarget.AllBuffered, playerbody.forward, moveDirection);
            playerbody.forward = Vector3.Lerp(playerbody.forward, moveDirection, 1);
            rigid.transform.position += Vector3.ClampMagnitude(moveDirection, 1f) * finalSpeed * Time.deltaTime;
            if (finalSpeed == speed && !jumping)
            {
                walkingSound();
            }
            else if (finalSpeed == runSpeed && !jumping)
            {
                runSound();
            }
        }

        float percent = ((run) ? 1 : 0.5f) * moveInput.magnitude;

        if (!PickupDrop.GetisCarrying())
        {
            anim.SetBool("IsCarrying", false);
            anim.SetFloat("Blend", percent, 0.1f, Time.deltaTime);
        }
        else
        {
            anim.SetBool("IsCarrying", true);
            anim.SetFloat("Blend", percent, 0.1f, Time.deltaTime);
        }
        moveInput = Vector2.zero;
    }

    void Jump()
    {
        if (jumpDown && !jumping)
        {
            anim.SetBool("IsJumping", true);
            rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumping = true;
            jumpSound();
        }
    }

    [PunRPC]
    void front(Vector3 front, Vector3 direction)
    {
        playerbody.forward = front;
        moveDirection = direction;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("IsJumping", false);
            jumping = false;
            jumpDown = false;
        }

        if (collision.gameObject.tag == "Wall" && jumping)
        {
            hitWall = true;
            rigid.AddForce(Vector3.down * jumpForce * 1.5f, ForceMode.Impulse);
        }
        else
        {
            hitWall = false;
        }

        if (collision.gameObject.tag == "Box" || collision.gameObject.tag == "Button")
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Box" || collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Button")
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Box" || collision.gameObject.tag == "Button")
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Dead")
        {
            Die();
        }

        if (other.tag == "Victory")
        {
            Clear();
        }
    }

    void walkingSound()
    {
        audioSource.clip = walk;
        audioSource.volume = 3.0f;
        audioSource.pitch = 1.3f;
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void runSound()
    {
        audioSource.clip = walk;
        audioSource.volume = 3.0f;
        audioSource.pitch = 1.95f;
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void jumpSound()
    {
        audioSource.clip = walk;
        audioSource.volume = 8.0f;
        audioSource.pitch = 1.0f;
        audioSource.Play();
    }

    void dieSound()
    {
        if (PV.IsMine)
        {
            audioSource.clip = die;
            audioSource.volume = 2.0f;
            audioSource.pitch = 1.0f;
            audioSource.Play();
        }
    }

    public void Spawn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        transform.position = startVector;
        PV.RPC("returnBlend", RpcTarget.All);
        isDead = false;
        GPanel.SetActive(false);
    }

    [PunRPC]
    void returnBlend()
    {
        anim.Play("Blend Tree");
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
    }

    [PunRPC]
    void Name(string name)
    {
        other = name;
    }

    [PunRPC]
    void DieMotion()
    {
        anim.Play("Die");
    }

    public bool getisDie() 
    {
        return isDead;
    }

    void Die() 
    {
        if (!isDead)
        {
            PV.RPC("DieMotion", RpcTarget.AllBuffered);
            dieSound();
        }
        isDead = true;
        if (PV.IsMine)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GPanel.SetActive(true);
        }
    }

    void Clear() 
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (PV.IsMine)
        {
            CPanel.SetActive(true);
            isDead = true;
            if ((PhotonNetwork.IsMasterClient))
            {
                MasterName.text = PhotonNetwork.NickName;
                OtherName.text = other;
            }
            else 
            {
                MasterName.text = other;
                OtherName.text = PhotonNetwork.NickName;
            }
            MasterName.color = Color.green;
            OtherName.color = Color.red;
        }
    }

    public void QuitB()
    {
        Application.Quit();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
        }
    }
}
