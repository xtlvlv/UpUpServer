namespace UpUpServer;

public class AutoRouterAttribute: Attribute
{
    public uint ReqId;
    public AutoRouterAttribute(uint reqId)
    {
        ReqId = reqId;
    }
}