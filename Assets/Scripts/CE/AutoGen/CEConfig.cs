//////////////////////////////////////////////////////////////////////////
/// This is an auto-generated script, please do not modify it manually ///
//////////////////////////////////////////////////////////////////////////

using System.Text;
using System.Collections;
using System.Collections.Generic;
using CE;

public sealed class CEConfig : ICELoader
{
    public static readonly string CEName = "CEConfig";

    public string Id { get; private set; }
    public string Value { get; private set; }

    public void Load(Hashtable ht)
    {
        Id = CEConvertHelper.O2STrim(ht["Id"]);
        Value = CEConvertHelper.O2STrim(ht["Value"]);
    }

    public static CEConfig GetElement(string elementKey)
    {
        return CEManager.instance.GetElementString(CEName, elementKey) as CEConfig;
    }

    public static Dictionary<string, ICELoader> GetElementDict()
    {
        return CEManager.instance.GetDictString(CEName);
    }

    public CEConfig Clone()
    {
        var clone = new CEConfig();
        clone.Id = Id;
        clone.Value = Value;
        return clone;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(CEName).Append("->");
        sb.AppendLine();
        sb.Append("Id: ").Append(Id);
        sb.AppendLine();
        sb.Append("Value: ").Append(Value);
        sb.AppendLine();
        return sb.ToString();
    }
}
