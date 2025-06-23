using Unity.Netcode;
using UnityEngine;

public struct InputState : INetworkSerializable
{
    public int tick;
    public Vector2 move;
    public bool jump;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref move);
        serializer.SerializeValue(ref jump);
    }
}
