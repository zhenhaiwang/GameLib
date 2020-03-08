//////////////////////////////////////////////////////////////////////////
/// This is an auto-generated script, please do not modify it manually ///
//////////////////////////////////////////////////////////////////////////

using System.Text;
using System.Collections;
using System.Collections.Generic;
using CE;

public sealed class CEAutoLoad : ICELoader
{
    public static readonly string CEName = "CEAutoLoad";

    public string SheetName { get; private set; }
    public int KeyType { get; private set; }

    public void Load(Hashtable ht)
    {
        SheetName = CEConvertHelper.O2STrim(ht["SheetName"]);
        KeyType = CEConvertHelper.O2I(ht["KeyType"]);
    }

    public static CEAutoLoad GetElement(string elementKey)
    {
        return CEManager.instance.GetElementString(CEName, elementKey) as CEAutoLoad;
    }

    public static Dictionary<string, ICELoader> GetElementDict()
    {
        return CEManager.instance.GetDictString(CEName);
    }

    public CEAutoLoad Clone()
    {
        var clone = new CEAutoLoad();
        clone.SheetName = SheetName;
        clone.KeyType = KeyType;
        return clone;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(CEName).Append("->");
        sb.AppendLine();
        sb.Append("SheetName: ").Append(SheetName);
        sb.AppendLine();
        sb.Append("KeyType: ").Append(KeyType);
        sb.AppendLine();
        return sb.ToString();
    }
}
