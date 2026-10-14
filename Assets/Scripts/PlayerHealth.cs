using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public void Kill()
    {
        if (!IsServer) return;
        Debug.Log(name + " was killed");
    }
}
