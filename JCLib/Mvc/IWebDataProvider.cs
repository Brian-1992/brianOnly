using JCLib.DB;

namespace JCLib.Mvc
{
    public interface IWebDataProvider
    {
        string UserInfo { get; }
        string ProcUser { get; }
        string ProcIP { get; }
        string PageIndex { get; }
        string PageSize { get; }
        string Sort { get; }
    }
}
