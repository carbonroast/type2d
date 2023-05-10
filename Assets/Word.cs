using Unity.Collections;
using Unity.Netcode;
using System.Collections.Generic;


[System.Serializable]
public struct Word : INetworkSerializable
{    

    public string phrase;
    public float speed;
    public ulong clientId;
    public float damage;

    public float pointValue;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref phrase);
        serializer.SerializeValue(ref speed);
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref pointValue);
        serializer.SerializeValue(ref damage);
    }





    // Array version maybe use later
    //  public string[] wordArray;
    // public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    // {
    //     // Length
    //     int length = 0;
    //     if (!serializer.IsReader)
    //     {
    //         length = wordArray.Length;
    //     }

    //     serializer.SerializeValue(ref length);

    //     // Array
    //     if (serializer.IsReader)
    //     {
    //         wordArray = new string[length];
    //     }

    //     for (int n = 0; n < length; ++n)
    //     {
    //         serializer.SerializeValue(ref wordArray[n]);
    //     }

    //     serializer.SerializeValue(ref speed);
    //     serializer.SerializeValue(ref clientId);
    // }

    // public Word (string[] wordArray, ulong clientId)
    // {
    //     this.wordArray = wordArray;
    //     this.clientId = clientId;
    // }

}
