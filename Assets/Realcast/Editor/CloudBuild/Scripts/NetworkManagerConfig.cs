using UnityEngine;

[CreateAssetMenu(menuName = "Config/Network/NetworkManagerConfig")]
public class NetworkManagerConfig : ScriptableObject
{
    public enum PlayerPrefabType
    {
        Player,
        Bot
    }

    public PlayerPrefabType PrefabType;
}
