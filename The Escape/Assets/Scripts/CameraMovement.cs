using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CameraMovement : MonoBehaviourPunCallbacks
{
    public Transform objectTofollow;
    public float followSpeed = 35f;
    public float sensitivity = 100f;
    public float clampAngle = 70f;
    public float minDistance;
    public float maxDistance;
    public float finalDistance;
    public float smoothness = 10f;
    public Player player;
    public PhotonView PV;

    private float rotX;
    private float rotY;

    public Transform realCamera;
    public Vector3 dirNormalized;
    public Vector3 finalDir;
    LayerMask mask;

    // Start is called before the first frame update
    void Start()
    {
        SpawnSetting();
        rotX = transform.localRotation.eulerAngles.x;
        rotY = transform.localRotation.eulerAngles.y;

        dirNormalized = realCamera.localPosition.normalized;

        finalDistance = realCamera.localPosition.magnitude;

        mask = LayerMask.GetMask("Box") | LayerMask.GetMask("Player");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (PV.IsMine)
        {
            if (!player.getisDie() && objectTofollow != null)
            {
                rotX += -1 * (Input.GetAxis("Mouse Y")) * sensitivity * Time.deltaTime;
                rotY += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;

                rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);
                Quaternion rot = Quaternion.Euler(rotX, rotY, 0);

                transform.rotation = rot;

                transform.position = Vector3.MoveTowards(transform.position, objectTofollow.position, followSpeed * Time.deltaTime);

                finalDir = transform.TransformPoint(dirNormalized * maxDistance);

                RaycastHit hit;

                if (Physics.Linecast(transform.position, finalDir, out hit, ~mask))
                {
                    finalDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
                }
                else
                {
                    finalDistance = maxDistance;
                }

                realCamera.localPosition = Vector3.Lerp(realCamera.localPosition, dirNormalized * finalDistance, Time.deltaTime * smoothness);
            }
        }
        
    }

    public void SpawnSetting() 
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject.Find("Camera").transform.FindChild("Main CameraG").gameObject.SetActive(true);
            realCamera = GameObject.Find("Main CameraG").transform;
            objectTofollow = GameObject.Find("FollowCamG").transform;
        }
        else
        {
            GameObject.Find("Camera").transform.FindChild("Main CameraR").gameObject.SetActive(true);
            realCamera = GameObject.Find("Main CameraR").transform;
            objectTofollow = GameObject.Find("FollowCamR").transform;
        }
    }
}
