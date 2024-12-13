using System;

namespace MédiaPlayer
{
    public interface ISerializable
    {
        string Serialize();
        void Deserialize(string json);
    }
}
