using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class CameraController : NetworkBehaviour
{
    public GameObject cameraHolder;
    public void Update()
    {
        if(!IsOwner) return;
        cameraHolder.SetActive(true);
    }
}
