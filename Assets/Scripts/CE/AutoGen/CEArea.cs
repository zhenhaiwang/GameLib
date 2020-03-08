//////////////////////////////////////////////////////////////////////////
/// This is an auto-generated script, please do not modify it manually ///
//////////////////////////////////////////////////////////////////////////

using System.Text;
using System.Collections;
using System.Collections.Generic;
using CE;

public sealed class CEArea : ICELoader
{
    public static readonly string CEName = "CEArea";

    public int AreaId { get; private set; }
    public string AreaName { get; private set; }

    public void Load(Hashtable ht)
    {
        AreaId = CEConvertHelper.O2I(ht["AreaId"]);
        AreaName = CEConvertHelper.O2STrim(ht["AreaName"]);
    }

    public static CEArea GetElement(int elementKey)
    {
        return CEManager.instance.GetElementInt(CEName, elementKey) as CEArea;
    }

    public static Dictionary<int, ICELoader> GetElementDict()
    {
        return CEManager.instance.GetDictInt(CEName);
    }

    public CEArea Clone()
    {
        var clone = new CEArea();
        clone.AreaId = AreaId;
        clone.AreaName = AreaName;
        return clone;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(CEName).Append("->");
        sb.AppendLine();
        sb.Append("AreaId: ").Append(AreaId);
        sb.AppendLine();
        sb.Append("AreaName: ").Append(AreaName);
        sb.AppendLine();
        return sb.ToString();
    }
}
