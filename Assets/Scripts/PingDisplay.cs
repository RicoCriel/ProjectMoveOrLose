using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PingDisplay : MonoBehaviour
{
    public TextMeshProUGUI pingText;

    private void Update()
    {
        // Check if connected to Photon server
        if (PhotonNetwork.IsConnected)
        {
            // Get the ping from Photon
            int ping = PhotonNetwork.GetPing();

            // Update the UI text
            pingText.text = "Ping: " + ping.ToString() + "ms";
        }
    }
}
