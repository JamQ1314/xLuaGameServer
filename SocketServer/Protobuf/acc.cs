//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: acc.proto
namespace acc
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"Account")]
  public partial class Account : global::ProtoBuf.IExtensible
  {
    public Account() {}
    
    private int _id;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int id
    {
      get { return _id; }
      set { _id = value; }
    }
    private string _acc;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"acc", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string acc
    {
      get { return _acc; }
      set { _acc = value; }
    }
    private string _pwd;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"pwd", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string pwd
    {
      get { return _pwd; }
      set { _pwd = value; }
    }
    private byte[] _ico;
    [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name=@"ico", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public byte[] ico
    {
      get { return _ico; }
      set { _ico = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}