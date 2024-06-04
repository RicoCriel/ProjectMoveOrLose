using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace DefaultNamespace.PhotonRooms
{
    public class ConnectToServerRooms : MonoBehaviourPunCallbacks
    {
        public TMP_InputField userNameInput;
        public TextMeshProUGUI ButtonText;
        
        public string SceneToLoad = "Lobby";
        
        public void OnClickConnect ()
        {
            if (userNameInput.text.Length >= 1)
            {
                PhotonNetwork.NickName = userNameInput.text;
                ButtonText.text = "Connecting...";
                PhotonNetwork.AutomaticallySyncScene = true;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        
        public override void OnConnectedToMaster()
        {
            SceneManager.LoadScene(SceneToLoad);
        }
    }
}
