using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OverNetworkData : MonoBehaviourPun, IPunObservable
{
    public bool gameActive;



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameActive);
        }

        if (stream.IsReading)
        {
            gameActive = (bool)stream.ReceiveNext();
        }
    }

  
}
