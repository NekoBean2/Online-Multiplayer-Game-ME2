using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class NetworkDebugger : MonoBehaviour
{
    void Start()
    {
        string hostName = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(hostName);
        IPAddress ipAddr = ipHost.AddressList[0];
        Debug.Log("Host IP Address: " + ipAddr.ToString());
    }
}