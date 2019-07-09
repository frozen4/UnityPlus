using System;
using Hoba.ObjectPool;

public class CS2CPrtcData : PooledObject
{
	public int Type = 0;
	public byte[] Buffer = null;
    public long TimeStamp = 0;

    public CS2CPrtcData() { }

    public void Set(int t, byte[] data)
	{
		Type = t;
		Buffer = data;
        TimeStamp = DateTime.Now.Ticks;
    }

    protected override void OnResetState()
    {
        Set(0, null);
    }
}