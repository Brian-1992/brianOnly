namespace JCLib.Mvc
{
    /// <summary>
    /// 【分頁資訊物件】
    /// 1.建構子初始化，先傳入字串形式參數
    /// 2.要用到的時候再做轉換
    /// 3.轉換失敗時，目前頁數為1，每頁筆數為20
    /// 4.屬性設計為唯讀
    /// </summary>
    public class PagingInfo
    {
        private bool _parsed = false;
        private string _pageIndexStr;
        private string _pageSizeStr;
        private string _sortStr;
        private int _pageIndex;
        private int _pageSize;
        private string _sort;
        public PagingInfo(string pageIndex, string pageSize, string sort)
        {
            this._pageIndexStr = pageIndex;
            this._pageSizeStr = pageSize;
            this._sortStr = sort;
        }
        private void TryParse()
        {
            if (!_parsed)
            {
                if (!int.TryParse(_pageIndexStr, out _pageIndex)) _pageIndex = 1;
                if (!int.TryParse(_pageSizeStr, out _pageSize)) _pageSize = 20;
                _sort = _sortStr ?? "";
                _parsed = true;
            }
        }
        public int PageIndex { get { TryParse(); return _pageIndex; } }
        public int PageSize { get { TryParse(); return _pageSize; } }
        public string Sort { get { TryParse(); return _sort; } }
    }
}
