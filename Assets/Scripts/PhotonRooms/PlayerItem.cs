using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using Photon.Pun;
using System;
namespace DefaultNamespace.PhotonRooms
{
    public class PlayerItem : MonoBehaviourPunCallbacks
    {
        public TextMeshProUGUI playerName;

        [SerializeField]
        private Image playerImage;
        [SerializeField]
        private Image playerImageHighLight;
        public Color highlightColor;
        [SerializeField]
        private GameObject LeftArrowButton;
        [SerializeField]
        private GameObject RightArrowButton;

        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
        public Image playerAvatar;
        public Sprite[] Avatars;

        Player player;


        private void Awake()
        {
            // throw new NotImplementedException();
        }

        public void SetPlayerInfo(Player _player)
        {
            playerName.text = _player.NickName;
            player = _player;
            UpdatePlayerItem(_player);
        }

        public void ApplyLocalChanges()
        {
            playerImageHighLight.color = highlightColor;
            RightArrowButton.SetActive(true);
            LeftArrowButton.SetActive(true);
        }

        public void OnClickLeftArrow()
        {
            if ((int)playerProperties["playerAvatar"] == 0)
            {
                playerProperties["playerAvatar"] = Avatars.Length - 1;
            }
            else
            {
                playerProperties["playerAvatar"] = (int)playerProperties["playerAvatar"] - 1;
            }
            PhotonNetwork.SetPlayerCustomProperties(playerProperties);
        }

        public void OnClickRightArrow()
        {
            if ((int)playerProperties["playerAvatar"] == Avatars.Length - 1)
            {
                playerProperties["playerAvatar"] = 0;
            }
            else
            {
                playerProperties["playerAvatar"] = (int)playerProperties["playerAvatar"] + 1;
            }
            PhotonNetwork.SetPlayerCustomProperties(playerProperties);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if (player == targetPlayer)
            {
                UpdatePlayerItem(targetPlayer);
            }
        }
        private void UpdatePlayerItem(Player targetPlayer)
        {
            if (targetPlayer.CustomProperties.TryGetValue("playerAvatar", out object property))
            {
                playerAvatar.sprite = Avatars[(int)property];
                playerProperties["playerAvatar"] = (int)property;
            }
            else
            {
                playerProperties["playerAvatar"] = 0;
            }
        }
    }
}
