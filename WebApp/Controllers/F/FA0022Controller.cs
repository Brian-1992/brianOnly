using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using WebApp.Repository.F;
using WebApp.Models;
using Newtonsoft.Json;
using JCLib.DB;

namespace WebApp.Controllers.F
{
    public class FA0022Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    FA0022Repository repo = new FA0022Repository(DBWork);
                    DataTable dt = new DataTable();
                    dt.Columns.Add("ITEM_NAME");        // 統計分類
                    dt.Columns.Add("AMT_1");            // 藥品類
                    dt.Columns.Add("AMT_2");            // 消耗性醫療器材類
                    dt.Columns.Add("REMARK");
                    dt.Rows.Add(SetRow(dt, "當月醫療總收入金額(A)", "--------", "--------", ""));

                    IEnumerable<AA0073M> myEnum = repo.GetAll(form.Get("DATA_YM"));
                    myEnum.GetEnumerator();
                    foreach (var item in myEnum)
                    {
                        for (int i = 1; i < 10; i++)
                        {
                            bool flag = false;
                            if (item.MAT_CLASS.Contains("$"))
                                flag = true;

                            if (i == 1)
                                AddRow(ref dt, flag, "當月期初結存金額(B)", item.MAT_CLASS, item.P_INV_AMT);
                            else if (i == 2)
                                AddRow(ref dt, flag, "當月期末結存金額(C)", item.MAT_CLASS, item.INV_AMT);
                            else if (i == 3)
                            {
                                double b = (Convert.ToDouble(dt.Rows[1][2].ToString()) + Convert.ToDouble(dt.Rows[2][2].ToString())) / 2;
                                dt.Rows.Add(SetRow(dt, "當月平均庫儲金額<br>(D)=(B+C)/2", "--------", b.ToString("f2"), ""));
                            }
                            else if (i == 4)
                                AddRow(ref dt, flag, "當月消耗金額(E)", item.MAT_CLASS, item.USE_AMT);
                            else if (i == 5)
                            {
                                double b = 0;

                                if (Convert.ToDouble(dt.Rows[4][2].ToString()) == 0)
                                    b = 0;  // 分母為零
                                else
                                    b = Convert.ToDouble(dt.Rows[2][2].ToString()) / Convert.ToDouble(dt.Rows[4][2].ToString());
                                dt.Rows.Add(SetRow(dt, "當月期末比值<br>(F)=(C/E)", "--------", b.ToString("f2"), ""));
                            }
                            else if (i == 6)
                            {
                                double b = 0;

                                if (Convert.ToDouble(dt.Rows[3][2].ToString()) == 0)
                                    b = 0;  // 分母為零
                                else
                                    b = Convert.ToDouble(dt.Rows[4][2].ToString()) / Convert.ToDouble(dt.Rows[3][2].ToString());
                                dt.Rows.Add(SetRow(dt, "當月存貨週轉率<br>(G)=(E/D)", "--------", b.ToString("f2"), ""));
                            }
                            else if (i == 7)
                                dt.Rows.Add(SetRow(dt, "當月消耗金額佔醫療收入之百分比<br>(H)=(E/A)*100%", "--------", "--------", ""));
                            else if (i == 8)
                                AddRow(ref dt, flag, "當月低週轉率品項結存金額(I)", item.MAT_CLASS, item.LOWTURN_INV_AMT);
                            else if (i == 9)
                            {
                                double b = 0;

                                if (Convert.ToDouble(dt.Rows[4][2].ToString()) == 0)
                                    b = 0;  // 分母為零
                                else
                                    b = Convert.ToDouble(dt.Rows[8][2].ToString()) / Convert.ToDouble(dt.Rows[4][2].ToString()) * 100;
                                dt.Rows.Add(SetRow(dt, "當月低週轉率品項之百分比<br>(J)=(I/E)*100%", "--------", b.ToString("f2") + "%", ""));

                            }
                        }
                    }

                    session.Result.etts = ConvertToAA0073Model(dt);
                    //session.Result.etts = repo.GetAll(form.Get("DATA_YM"));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        protected void AddRow(ref DataTable dt, bool flag, string item_name, string mat_class, string value)
        {
            if (flag)
            {
                string[] tmp = value.Split('$');
                dt.Rows.Add(SetRow(dt, item_name, "--------", tmp[1], ""));
            }
            else
                dt.Rows.Add(SetRow(dt, item_name, "--------", value, ""));
        }

        protected DataRow SetRow(DataTable dt, string item_name, string amt_1, string amt_2, string remark)
        {
            DataRow dr = dt.NewRow();
            dr["ITEM_NAME"] = item_name;
            dr["AMT_1"] = amt_1;
            dr["AMT_2"] = amt_2;
            dr["REMARK"] = remark;

            return dr;
        }

        [HttpPost]
        public ApiResponse GetYmCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0022Repository repo = new FA0022Repository(DBWork);
                    session.Result.etts = repo.GetYmCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public IEnumerable<AA0073M> ConvertToAA0073Model(DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                yield return new AA0073M
                {
                    ITEM_NAME = Convert.ToString(row["ITEM_NAME"]),
                    AMT_1 = Convert.ToString(row["AMT_1"]),
                    AMT_2 = Convert.ToString(row["AMT_2"]),
                    REMARK = Convert.ToString(row["REMARK"])
                };
            }

        }
    }
}
