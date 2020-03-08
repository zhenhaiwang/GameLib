//////////////////////////////////////////////////////////////////////////
/// This is an auto-generated script, please do not modify it manually ///
//////////////////////////////////////////////////////////////////////////

using CE;

public static class CEHashHelper
{
    public static ICELoader CreateLoaderFromHash(uint hash)
    {
        ICELoader loader = null;

        switch (hash)
        {
            case 1647651811:
                {
                    loader = new CEArea();
                }
                break;
            case 420587931:
                {
                    loader = new CEAutoLoad();
                }
                break;
            case 1283261664:
                {
                    loader = new CEConfig();
                }
                break;
        }

        return loader;
    }
}
