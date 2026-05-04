using System.Collections.Generic;

// Пользовательский компаратор равенства для NetworkedPlayerInfo, использующий ClientId
// Позволяет надежно сравнивать в коллекциях, даже если косметика, цвет и т.д. изменяются
public sealed class NetPlayerInfoCidComparer : IEqualityComparer<NetworkedPlayerInfo>
{
    public bool Equals(NetworkedPlayerInfo data1, NetworkedPlayerInfo data2)
    {
        return data1.ClientId == data2.ClientId;
    }

    public int GetHashCode(NetworkedPlayerInfo data)
    {
        return data.ClientId;
    }
}
