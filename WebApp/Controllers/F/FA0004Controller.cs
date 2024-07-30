using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using WebApp.Models;
using WebApp.Repository.F;

namespace WebApp.Controllers.F
{
    public class FA0004Controller : SiteBase.BaseApiController
    {

        public ApiResponse All(FormDataCollection form) {
            var chk_ym = form.Get("P0");
            var matClass = form.Get("P1");
            var storeId = form.Get("P2");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0004Repository(DBWork);
                    //session.Result.etts = repo.GetAll(chk_ym, storeId, matClass);
                    IEnumerable<FA0004M> list = repo.GetAll(chk_ym, storeId, matClass);
                    list = GetGroup(list);
                    list = GetCompleteData(list, false);

                    session.Result.etts = list;

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public IEnumerable<FA0004M> GetGroup(IEnumerable<FA0004M> list) {
            var o = from a in list
                    group a by new
                    {
                        MMCODE = a.MMCODE,
                        MMNAME_C = a.MMNAME_C,
                        MMNAME_E = a.MMNAME_E,
                        // STATUS_TOT = a.STATUS_TOT,
                        //CHK_QTY1 = a.CHK_QTY1,
                        //CHK_QTY2 = a.CHK_QTY2,
                        //CHK_QTY3 = a.CHK_QTY3,
                        //APL_OUTQTY = a.APL_OUTQTY,
                        //STORE_QTY = a.STORE_QTY,
                        //M_CONTPRICE = a.M_CONTPRICE
                    } into g
                    select new FA0004M
                    {
                        MMCODE = g.Key.MMCODE,
                        MMNAME_C = g.Key.MMNAME_C,
                        MMNAME_E = g.Key.MMNAME_E,
                        // STATUS_TOT = g.Select(x => x.STATUS_TOT).ToString(),
                        //CHK_QTY1 = g.Key.CHK_QTY1,
                        //CHK_QTY2 = g.Key.CHK_QTY2,
                        // CHK_QTY3 = g.Key.CHK_QTY3,
                        //APL_OUTQTY = g.Key.APL_OUTQTY,
                        //STORE_QTY = g.Key.STORE_QTY,
                        // M_CONTPRICE = g.Key.M_CONTPRICE,
                        items = g.OrderByDescending(x => int.Parse(x.STATUS_TOT)).ToList()
                    };

            return o.ToList();
        }

        public IEnumerable<FA0004M> GetCompleteData(IEnumerable<FA0004M> list, bool isExcel) {
            string lineBreak = isExcel ?"; " : "<br>";
            foreach (FA0004M item in list)
            {
                FA0004M temp = item.items.Select(x => x).First();
                item.CHK_QTY1 = temp.CHK_QTY1;
                item.CHK_QTY2 = temp.CHK_QTY2;
                item.CHK_QTY3 = temp.CHK_QTY3;
                item.STATUS_TOT = temp.STATUS_TOT;
                item.APL_OUTQTY = temp.APL_OUTQTY;
                item.M_CONTPRICE = temp.M_CONTPRICE;
                item.STORE_QTY = temp.STORE_QTY;
                item.COMMENT = string.Empty;

                foreach (FA0004M innerItem in item.items)
                {
                    if (innerItem.CHK_REMARK != string.Empty)
                    {
                        if (item.COMMENT != string.Empty)
                        {
                            item.COMMENT += lineBreak;
                        }

                        item.COMMENT += string.Format("{0} {1}", innerItem.STORE_LOC, innerItem.CHK_REMARK);
                    }
                }

                float chk_qty = item.STATUS_TOT == "1" ? float.Parse(item.CHK_QTY1) :
                                item.STATUS_TOT == "2" ? float.Parse(item.CHK_QTY2) :
                                                         float.Parse(item.CHK_QTY3);
                float apl_outqty = item.APL_OUTQTY == null ? 0 : float.Parse(item.APL_OUTQTY);
                item.DIFF_QTY = (chk_qty - float.Parse(item.STORE_QTY)).ToString();
                item.DIFF_COST = (float.Parse(item.DIFF_QTY) * float.Parse(item.M_CONTPRICE)).ToString();
                item.DIFF_PERC = apl_outqty == 0 ? "0" :
                                 Math.Round((float.Parse(item.DIFF_QTY) / apl_outqty * 100), 2).ToString() + "%";

            }

            return list;
        }

        public ApiResponse Excel(FormDataCollection form) {
            var chk_ym = form.Get("P0");
            var matClass = form.Get("P1");
            var storeId = form.Get("P2");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0004Repository(DBWork);
                    //session.Result.etts = repo.GetAll(chk_ym, storeId, matClass);
                    IEnumerable<FA0004M> list = repo.GetExcel(chk_ym, storeId, matClass);
                    list = GetGroup(list);
                    list = GetCompleteData(list, true);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;
                    DataTable temp = GetDataTable(list);
                    dtItems.Merge(temp);

                    JCLib.Excel.Export(form.Get("FN"), dtItems);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public DataTable GetDataTable(IEnumerable<FA0004M> list) {
            var props = typeof(FA0004M).GetProperties();
            var dt = new DataTable();

            List<string> columnList = new List<string>() {
                "MMCODE","MMNAME_C","MMNAME_E","COMMENT","DIFF_QTY","DIFF_COST","DIFF_PERC"
            };
            List<string> columnListName = new List<string>() {
                "院內碼","中文品名","英文品名","差異說明","差異量","差異成本","百分比"
            };

            foreach (string column in columnList) {
                dt.Columns.Add(column);
            }

            object[] values = new object[columnList.Count];
            foreach (FA0004M item in list) {
                for (int i = 0; i < columnList.Count(); i++) {
                    values[i] = item.GetType().GetProperty(columnList[i]).GetValue(item, null);
                }
                dt.Rows.Add(values);
            }

            for (int i = 0; i < columnList.Count(); i++) {
                dt.Columns[i].ColumnName = columnListName[i];
            }

            return dt;
        }

        #region combo

        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0004Repository(DBWork);
                    session.Result.etts = repo.GetMatclassCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #endregion
    }
}