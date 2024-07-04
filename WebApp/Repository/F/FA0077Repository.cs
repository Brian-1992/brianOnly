using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.F
{
    public class FA0077Repository : JCLib.Mvc.BaseRepository
    {
        public FA0077Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public string GetPreym()
        {
            string sql = @"select TWN_PYM(TWN_YYYMM(sysdate)) from dual";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public int DeleteAbsFee()
        {
            string sql = @"delete from MM_ABS_FEE
                            where data_ym = twn_pym(twn_yyymm(sysdate))";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }
        public int insertAbsFee()
        {
            string sql = @"insert into MM_ABS_FEE(data_ym, inid, abs_amount, create_date, create_user)
                           select data_ym, inid, sum(use_amount) as abs_amount,
                                  sysdate as create_date, '不計價衛材' as create_user
                             from (
                                    select b.data_ym, b.inid,
                                           (b.DISC_CPRICE * b.use_qty) as USE_AMOUNT
                                      from (
                                             select mmcode from MI_MAST
                                              where m_paykind = '3'
                                                and mat_class = '02'
                                           )A
                                    inner join
                                        (select DATA_YM,
                                                (select INID from MI_WHMAST where WH_NO=a.WH_NO) as INID,
                                                MMCODE,
                                                (select DISC_CPRICE from MI_WHCOST 
                                                  where DATA_YM=a.DATA_YM and MMCODE=a.MMCODE
                                                    and DATA_YM=SET_YM 
                                                ) as DISC_CPRICE,
                                                USE_QTY
                                           from MI_WINVMON a
                                          where 1=1
                                            and DATA_YM= twn_pym(twn_yyymm(sysdate))
                                            and (select 1 from MI_WHMAST where wh_no = a.wh_no and cancel_id = 'N') = 1
                                        ) B
                                    on A.mmcode = B.mmcode
                                  )
                            group by data_ym, inid
                           having data_ym is not null
                            order by data_ym, inid";

            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public FA0077DataYmItem GetDataYmItem()
        {
            string sql = @"select TWN_YYYMM(trunc(add_months(add_months(trunc(add_months(trunc(sysdate,'MM'),-1),'Y')-1,
                                            to_number(to_char(add_months(sysdate,-1),'Q'))*3),-2),'MM')) as mindataym, 
                                  TWN_YYYMM(add_months(trunc(sysdate,'MM'),-1)) as maxdataym,  
                                  TWN_YYYMM(trunc(add_months(add_months(trunc(add_months(trunc(add_months(sysdate,-12),'MM'),-1),'Y')-1,
                                            to_number(to_char(add_months(trunc(add_months(sysdate,-12),'MM'),-1),'Q'))*3),-2),'MM')) as premindataym,  
                                  TWN_YYYMM(add_months(trunc(add_months(trunc(add_months(sysdate,-12),'MM'),-1),'Y') -1 ,
                                            to_number(to_char(add_months(trunc(add_months(sysdate,-12),'MM'),-1),'Q'))*3)) as premaxdataym,  
                                  TO_CHAR(TWN_TODATE(TWN_YYYMM(add_months(trunc(add_months(trunc(add_months(sysdate,-12),'MM'),-1),'Y') -1 ,
                                            to_number(to_char(add_months(trunc(add_months(sysdate,-12),'MM'),-1),'Q'))*3))||'01'), 'Q') as q, 
                                  to_number(to_char(add_months(sysdate, -12), 'YYYY')) - 1911 as pre_y,
                                  to_number(to_char(sysdate, 'YYYY')) - 1911 as y,
                                  TWN_YYYMM(trunc(add_months(add_months(trunc(add_months(trunc(sysdate,'MM'),-1),'Y')-1,
                                            to_number(to_char(add_months(sysdate,-1),'Q'))*3),-2),'MM')) as wk_date
                             from dual";

            FA0077DataYmItem item = DBWork.Connection.QueryFirstOrDefault<FA0077DataYmItem>(sql, DBWork.Transaction);

            if (IsLarger(item.MaxDataym, item.MinDataym))
            {
                item.WK_DATE = GetWkDate(item.MinDataym);
            }

            return item;
        }

        public string GetWkDate(string minDataym)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"select to_char(to_NUMBER(:mindataym)+1) as wk_date from dual";
            p.Add("mindataym", minDataym);

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MM_ABS_FEE> GetData(string data_ym1, string data_ym2, string grp_no, string inid)
        {

            var p = new DynamicParameters();
            string sql = string.Format(@"select * from MM_ABS_FEE a
                            where a.data_ym between :data_ym1 and :data_ym2
                              and exists (select 1 from UR_INID_REL where inid = a.inid {0})",
                              grp_no == string.Empty ? grp_no : " and grp_no = :grp_no ");
            if (inid != string.Empty)
            {
                sql += @"     and inid = :inid";
            }
            p.Add("data_ym1", data_ym1);
            p.Add("data_ym2", data_ym2);
            p.Add("grp_no", grp_no);
            p.Add("inid", inid);

            return DBWork.Connection.Query<MM_ABS_FEE>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<FA0077> GetPastData(string data_ym1, string data_ym2, string grp_no, string inid, string rate)
        {

            var p = new DynamicParameters();
            string sql = string.Format(@"select (select grp_no from UR_INID_REL where inid = a.inid) as grp_no,
                                                (select c.grp_name from UR_INID_REL b, UR_INID_GRP c 
                                                  where b.inid = a.inid and c.grp_no = b.grp_no) as grp_name,
                                                a.inid,
                                                (select inid_name from UR_INID where inid = a.inid) as inid_name,
                                                round(nvl(avg(abs_amount), 0)) as AVG_CONSUME_AMOUNT,
                                                round((nvl(avg(abs_amount), 0) * ({0} / 100))) as target
                                           from MM_ABS_FEE a
                                          where a.data_ym between :data_ym1 and :data_ym2
                                            and exists (select 1 from UR_INID_REL where inid = a.inid {1} ) ",
                                            rate,
                                            grp_no == string.Empty ? grp_no : " and grp_no = :grp_no ");
            if (inid != string.Empty)
            {
                sql += @"     and inid = :inid";
            }
            sql += @"       group by a.inid";
            p.Add("data_ym1", data_ym1);
            p.Add("data_ym2", data_ym2);
            p.Add("grp_no", grp_no);
            p.Add("inid", inid);
            p.Add("rate", rate);

            return DBWork.PagingQuery<FA0077>(sql, p, DBWork.Transaction);
        }

        public DataTable GetT1Excel(string data_ym, string grp_no, string inid, string rate, FA0077DataYmItem ymItem)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = string.Format(@"select (select GRP_NAME from UR_INID_GRP  where GRP_NO=X.GRP_NO) as ""歸戶"",
                                                INID as ""單位代碼"",
                                                (select INID_NAME from UR_INID where INID=X.INID) as ""單位名稱"",
                                                AvgAmt as ""{6}Q{7}平均消耗金額"",
                                                TarAmt as ""{8}Q{7}目標值"",
                                                AbsAmt1 as ""{9}消耗金額"",
                                                DifAmt1 as ""{9}差異金額"",
                                                --GrwRat1,
                                                (case when GrwRat1=0 then 
                                                           (case when AvgAmt=0 then ''
                                                                 else to_char(GrwRat1)
                                                            end) 
                                                      when GrwRat1>0 then to_char(GrwRat1)
                                                      when GrwRat1<0 then to_char(GrwRat1)
                                                 end)  as ""{9}成長率""
                                                
                                                ,(case when GrwRat1=0 then
                                                           (case when AvgAmt=0 then ''
                                                                 else '達標'
                                                            end) 
                                                      when GrwRat1>0 then '未達標'
                                                      when GrwRat1<0 then '達標'
                                                 end) as ""{9}達標否""
                                                {0}
                                                {1}
                                           from (select D.GRP_NO,A.INID,
                                                        B.AvgAmt,B.TarAmt,
                                                        C1.AbsAmt as AbsAmt1,
                                                        (C1.AbsAmt-B.AvgAmt) as DifAmt1,   
                                                        (case B.AvgAmt
                                                             when 0 then 0
                                                              else round(((C1.AbsAmt-B.AvgAmt)/B.AvgAmt),4)*100
                                                          end) as GrwRat1
                                                {2}
                                                {3}
                                                   from (select distinct INID as INID  --單位代碼
                                                           from MM_ABS_FEE
                                                        ) A
                                                 left join     
                                                 (select INID,
                                                         round(NVL(sum(ABS_AMOUNT)/3,0)) as AvgAmt,  --去年同季平均值
                                                         round(NVL((sum(ABS_AMOUNT)/3),0)*( :rate /100)) as TarAmt  --目標值
                                                    from MM_ABS_FEE
                                                   where DATA_YM>= :premindataym  --去年同季年月起(pre_start),例如 '10801' 
                                                     and DATA_YM<= :premaxdataym    --去年同季年月迄(pre_end) ,例如 '10803'
                                                   group by INID
                                                 ) B on A.INID=B.INID
                                                left join
                                                 UR_INID_REL D on A.INID=D.INID  
                                                left join
                                                (select INID,round(ABS_AMOUNT) as AbsAmt
                                                   from MM_ABS_FEE
                                                  where DATA_YM= :mindataym  --'統計年月',例如 '10901'
                                                ) C1 on A.INID=C1.INID
                                                {4}
                                                {5}
                                           where 1=1",
                                           GetSecondSelect1(ymItem), GetThirdSelect1(ymItem),
                                           GetSecondSelect2(ymItem), GetThirdSelect2(ymItem),
                                           GetSecondWhere(ymItem), GetThirdWhere(ymItem),
                                           ymItem.Pre_y,
                                           ymItem.Q,
                                           ymItem.Y,
                                           ymItem.MinDataym);


            if (grp_no != string.Empty)
            {
                sql += "  and grp_no = :grp_no";
            }
            if (inid != string.Empty)
            {
                sql += "  and A.inid = :inid";
            }

            sql += @" ) X  
                    where 1=1
                      and (select 1 from UR_INID_REL where inid = x.inid) = 1
                    order by grp_no, inid";

            p.Add(":grp_no", grp_no);
            p.Add(":inid", inid);
            p.Add(":premindataym", ymItem.PreMinDataym);
            p.Add(":premaxdataym", ymItem.PreMaxDataym);
            p.Add(":mindataym", ymItem.MinDataym);
            p.Add(":maxdataym", ymItem.MaxDataym);
            p.Add(":wk_date", ymItem.WK_DATE);
            p.Add(":rate", rate);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        private string GetSecondSelect1(FA0077DataYmItem ymItem)
        {
            if (IsLarger(ymItem.MaxDataym, ymItem.MinDataym) == false)
            {
                return string.Empty;
            }
            return string.Format(@", AbsAmt2 as ""{0}消耗金額"",
                                     DifAmt2 as ""{0}差異金額"",
                                   (case when GrwRat2=0 then 
                                         (case when AvgAmt=0 then ''
                                                     else to_char(GrwRat2)
                                                end) 
                                          when GrwRat2>0 then to_char(GrwRat2)
                                          when GrwRat2<0 then to_char(GrwRat2)
                                     end) as ""{0}成長率 (D/A)"",
                                    (case when GrwRat2=0 then 
                                               (case when AvgAmt=0 then ''
                                                     else '達標'
                                                end) 
                                          when GrwRat2>0 then '未達標'
                                          when GrwRat2<0 then '達標'
                                     end) as ""{0}達標否""", ymItem.WK_DATE);
        }
        private string GetThirdSelect1(FA0077DataYmItem ymItem)
        {
            if (IsLarger(ymItem.MaxDataym, ymItem.WK_DATE) == false)
            {
                return string.Empty;
            }
            return string.Format(@",AbsAmt3 as ""{0}消耗金額"",
                                    DifAmt3 as ""{0}差異金額"",
                                    (case when GrwRat3=0 then 
                                               (case when AvgAmt=0 then ''
                                                     else to_char(GrwRat3)
                                                end) 
                                          when GrwRat3>0 then to_char(GrwRat3)
                                          when GrwRat3<0 then to_char(GrwRat3)
                                     end) as ""{0}成長率 (D/A)"",
                                    (case when GrwRat3=0 then 
                                               (case when AvgAmt=0 then ''
                                                     else '達標'
                                                end) 
                                          when GrwRat3>0 then '未達標'
                                          when GrwRat3<0 then '達標'
                                     end) as ""{0}達標否""
                             ", ymItem.MaxDataym);
        }
        private string GetSecondSelect2(FA0077DataYmItem ymItem)
        {
            if (IsLarger(ymItem.MaxDataym, ymItem.MinDataym) == false)
            {
                return string.Empty;
            }
            return @",C2.AbsAmt as AbsAmt2,
                      (C2.AbsAmt-B.AvgAmt) as DifAmt2,   
                      (case B.AvgAmt
                            when 0 then 0
                            else round(((C2.AbsAmt-B.AvgAmt)/B.AvgAmt),4)*100
                       end) as GrwRat2
              ";
        }
        private string GetThirdSelect2(FA0077DataYmItem ymItem)
        {
            if (IsLarger(ymItem.MaxDataym, ymItem.WK_DATE) == false)
            {
                return string.Empty;
            }
            return @",C3.AbsAmt as AbsAmt3,
                      (C3.AbsAmt-B.AvgAmt) as DifAmt3,   
                      (case B.AvgAmt
                            when 0 then 0
                            else round(((C3.AbsAmt-B.AvgAmt)/B.AvgAmt),4)*100
                       end) as GrwRat3
              ";
        }
        private string GetSecondWhere(FA0077DataYmItem ymItem)
        {
            if (IsLarger(ymItem.MaxDataym, ymItem.MinDataym) == false)
            {
                return string.Empty;
            }
            return @"left join
                       (select INID,round(ABS_AMOUNT) as AbsAmt
                          from MM_ABS_FEE
                         where DATA_YM= :wk_date   --'統計年月'例如 '10902'
                       ) C2
                     on A.INID=C2.INID
                   ";
        }
        private string GetThirdWhere(FA0077DataYmItem ymItem)
        {
            if (IsLarger(ymItem.MaxDataym, ymItem.WK_DATE) == false)
            {
                return string.Empty;
            }
            return @"left join
                        (select INID,round(ABS_AMOUNT) as AbsAmt
                           from MM_ABS_FEE
                          where DATA_YM= :maxdataym   --'統計年月'例如 '10903'
                        ) C3
                      on A.INID=C3.INID
                    ";
        }

        private bool IsLarger(string a, string b)
        {
            return int.Parse(a) > int.Parse(b);
        }

        public IEnumerable<FA0077Detail> GetDetails(string data_ym_start, string data_ym_end, string grp_no, string inid, string mmcode)
        {
            var p = new DynamicParameters();
            string sql = @"
                select DATA_YM, grp_no,
                       (select GRP_NAME from UR_INID_GRP 
                         where GRP_NO=X.GRP_NO) as GRP_NAME,
                       INID, 
                       (select INID_NAME from UR_INID 
                         where INID=X.INID) as INID_NAME,
                       MMCODE,MMNAME_C,MMNAME_E,BASE_UNIT,
                       DISC_CPRICE,USE_QTY,USE_AMOUNT,
                       TURNOVER,
                       (case when M_STOREID=0 then '非庫備' else '庫備' end) as MSTOREID
                  from
                       (select B.DATA_YM,C.GRP_NO,B.INID, 
                              A.MMCODE,A.MMNAME_C,A.MMNAME_E,A.BASE_UNIT,
                              B.DISC_CPRICE,B.USE_QTY,
                              (B.DISC_CPRICE * B.USE_QTY) as USE_AMOUNT,
                              B.TURNOVER,
                              A.M_STOREID
                         from  --篩選院內碼
                           (select MMCODE,MMNAME_C,MMNAME_E,
                                   BASE_UNIT,M_STOREID
                              from MI_MAST
                             where M_PAYKIND='3'
                               and MAT_CLASS='02'";
            if (mmcode != string.Empty)
            {
                sql += string.Format(@"      and mmcode like '%{0}%'", mmcode);
            }
            sql += @"
                           ) A
                         left join  --找出月結的責任中心,庫存單價,消耗數量,週轉率
                           (select DATA_YM,
                                   (select INID from MI_WHMAST 
                                     where WH_NO=a.WH_NO
                                   ) as INID,  --責任中心
                                   MMCODE,
                                   (select DISC_CPRICE from MI_WHCOST 
                                     where DATA_YM=a.DATA_YM and MMCODE=a.MMCODE
                                       and DATA_YM=SET_YM 
                                   ) as DISC_CPRICE,  --每月月結合優惠約單價
                                   USE_QTY,TURNOVER
                              from MI_WINVMON a
                             where 1=1
                               and DATA_YM>= :data_ym_start and DATA_YM <= :data_ym_end
                           ) B
                         on A.MMCODE=B.MMCODE
                         left join  --找出責任中心對應的歸戶
                           UR_INID_REL C
                         on B.INID=C.INID
                       ) X
                       where 1=1 
                         and (select 1 from UR_INID_REL where INID = x.inid) = 1 ";
            if (grp_no != string.Empty)
            {
                sql += @"   and grp_no = :grp_no";
            }
            if (inid != string.Empty)
            {
                sql += @"   and inid = :inid";
            }
            sql += @"
                       order by DATA_YM,GRP_NO,INID,MMCODE 
            ";

            p.Add(":data_ym_start", data_ym_start);
            p.Add(":data_ym_end", data_ym_end);
            p.Add(":mmcode", mmcode);
            p.Add(":grp_no", grp_no);
            p.Add(":inid", inid);

            return DBWork.PagingQuery<FA0077Detail>(sql, p, DBWork.Transaction);
        }

        public DataTable GetT2Excel(string data_ym_start, string data_ym_end, string grp_no, string inid, string mmcode)
        {
            var p = new DynamicParameters();
            string sql = @"
                select DATA_YM as 年月, grp_no as 歸戶代碼,
                       (select GRP_NAME from UR_INID_GRP 
                         where GRP_NO=X.GRP_NO)  as 歸戶名稱,
                       INID as 責任中心, 
                       (select INID_NAME from UR_INID 
                         where INID=X.INID)  as 責任中心名稱,
                       MMCODE as 院內碼 ,MMNAME_C as 中文品名, MMNAME_E as 英文品名, BASE_UNIT as 計量單位,
                       DISC_CPRICE as 庫存單價 , USE_QTY as 消耗數量,USE_AMOUNT as 消耗金額,
                       TURNOVER as 週轉率,
                       (case when M_STOREID=0 then '非庫備' else '庫備' end) as  庫備否
                  from
                       (select B.DATA_YM,C.GRP_NO,B.INID, 
                              A.MMCODE,A.MMNAME_C,A.MMNAME_E,A.BASE_UNIT,
                              B.DISC_CPRICE,B.USE_QTY,
                              (B.DISC_CPRICE * B.USE_QTY) as USE_AMOUNT,
                              B.TURNOVER,
                              A.M_STOREID
                         from  --篩選院內碼
                           (select MMCODE,MMNAME_C,MMNAME_E,
                                   BASE_UNIT,M_STOREID
                              from MI_MAST
                             where M_PAYKIND='3'
                               and MAT_CLASS='02'";
            if (mmcode != string.Empty)
            {
                sql += string.Format(@"      and mmcode like '%{0}%'", mmcode);
            }
            sql += @"
                           ) A
                         left join  --找出月結的責任中心,庫存單價,消耗數量,週轉率
                           (select DATA_YM,
                                   (select INID from MI_WHMAST 
                                     where WH_NO=a.WH_NO
                                   ) as INID,  --責任中心
                                   MMCODE,
                                   (select DISC_CPRICE from MI_WHCOST 
                                     where DATA_YM=a.DATA_YM and MMCODE=a.MMCODE
                                       and DATA_YM=SET_YM 
                                   ) as DISC_CPRICE,  --每月月結優惠合約單價
                                   USE_QTY,TURNOVER
                              from MI_WINVMON a
                             where 1=1
                               and DATA_YM>= :data_ym_start and DATA_YM <= :data_ym_end
                           ) B
                         on A.MMCODE=B.MMCODE
                         left join  --找出責任中心對應的歸戶
                           UR_INID_REL C
                         on B.INID=C.INID
                       ) X
                       where 1=1 
                         and (select 1 from UR_INID_REL where inid = X.inid) = 1 ";
            if (grp_no != string.Empty)
            {
                sql += @"   and grp_no = :grp_no";
            }
            if (inid != string.Empty)
            {
                sql += @"   and inid = :inid";
            }
            sql += @"
                       order by DATA_YM,GRP_NO,INID,MMCODE 
            ";

            p.Add(":data_ym_start", data_ym_start);
            p.Add(":data_ym_end", data_ym_end);
            p.Add(":mmcode", mmcode);
            p.Add(":grp_no", grp_no);
            p.Add(":inid", inid);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        #region combos

        public IEnumerable<UR_INID_GRP> GetGrpNos(string queryString)
        {
            var p = new DynamicParameters();

            var sql = @"select {0} a.grp_no, 
                                  a.grp_name
                          from UR_INID_GRP a 
                         where 1=1 ";

            if (queryString != "")
            {
                sql = string.Format(sql, "(nvl(instr(a.grp_no, :grp_no), 1000) + nvl(instr(a.grp_name, :grp_name), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":grp_no", queryString);
                p.Add(":grp_name", queryString);

                sql += " and (a.grp_no like :grp_no ";
                p.Add(":grp_no", string.Format("%{0}%", queryString));

                sql += " or a.grp_name like :grp_name )";
                p.Add(":grp_name", string.Format("%{0}%", queryString));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.grp_no ";
            }

            return DBWork.PagingQuery<UR_INID_GRP>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<UR_INID> GetInids(string queryString, string grp_no)
        {
            var p = new DynamicParameters();

            var sql = @"select {0} a.inid, 
                                   b.inid_name
                          from UR_INID_REL a, UR_INID b  
                         where 1=1 
                           and a.inid = b.inid";

            if (grp_no != string.Empty)
            {
                sql += @"  and a.grp_no = :grp_no";
                p.Add(":grp_no", grp_no);
            }

            if (queryString != "")
            {
                sql = string.Format(sql, "(nvl(instr(a.inid, :inid), 1000) + nvl(instr(b.inid_name, :inid_name), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":inid", queryString);
                p.Add(":inid_name", queryString);

                sql += " and (a.inid like :inid ";
                p.Add(":inid", string.Format("%{0}%", queryString));

                sql += " or b.inid_name like :inid_name )";
                p.Add(":inid_name", string.Format("%{0}%", queryString));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.inid ";
            }

            return DBWork.PagingQuery<UR_INID>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E
                        FROM MI_MAST A 
                        WHERE 1=1 
                          and a.mat_class = '02'
                          and a.m_paykind = '3'";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR UPPER(A.MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR UPPER(A.MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        #endregion

        #region 2021-12-20 T3Grid
        public FA0077DataYmItem GetT3DataYmItem(string year, string q)
        {
            string sql = @"
                with base_month as (                             
                    select twn_todate(:year||lpad(:q*3-2,2,'0')||'01') as minQym
                        , twn_todate(:year||lpad(:q*3,2,'0')||'01') as maxQym
                    --, :q*3 as maxQym 
                    from dual
                )
                select twn_yyymm(minQym) as C_MIN_QYM, 
                       twn_yyymm(maxQym) as C_MAX_QYM,
                       to_number(to_char(maxQym, 'YYYY')) - 1911 as c_y,
                       to_char(maxQym, 'Q') as c_q,
                       twn_yyymm(add_months(minQym, -12)) as PRE_Y_MIN_QYM ,
                       twn_yyymm(add_months(maxQym, -12)) as PRE_Y_MAX_QYM ,
                       to_number(to_char(add_months(minQym, -12), 'YYYY')) - 1911 as PRE_Y_y,
                       to_char(add_months(minQym, -12), 'Q') as PRE_Y_q,
                       twn_yyymm(add_months(minQym, -3)) as PRE_S_MIN_QYM ,
                       twn_yyymm(add_months(maxQym, -3)) as PRE_S_MAX_QYM,
                       to_number(to_char(add_months(maxQym, -3), 'YYYY')) - 1911 as PRE_S_y,
                       to_char(add_months(maxQym, -3), 'Q') as PRE_S_q
                  from base_month a
            ";
            return DBWork.Connection.QueryFirstOrDefault<FA0077DataYmItem>(sql, new { year, q }, DBWork.Transaction);
        }
        public IEnumerable<FA0077> GetT3PastData(string data_ym1, string data_ym2, string grp_no, string inid, bool isPaging)
        {

            var p = new DynamicParameters();
            string sql = string.Format(@"select (select grp_no from UR_INID_REL where inid = a.inid) as grp_no,
                                                (select c.grp_name from UR_INID_REL b, UR_INID_GRP c 
                                                  where b.inid = a.inid and c.grp_no = b.grp_no) as grp_name,
                                                a.inid,
                                                (select inid_name from UR_INID where inid = a.inid) as inid_name,
                                                round(nvl(avg(abs_amount), 0), 2) as AVG_CONSUME_AMOUNT
                                           from MM_ABS_FEE a
                                          where a.data_ym between :data_ym1 and :data_ym2
                                            and exists (select 1 from UR_INID_REL where inid = a.inid {0} ) ",
                                            grp_no == string.Empty ? grp_no : " and grp_no = :grp_no ");
            if (inid != string.Empty)
            {
                sql += @"     and inid = :inid";
            }
            sql += @"       group by a.inid";
            if (isPaging == false)
            {
                sql += @"     order by a.inid";
            }
            p.Add("data_ym1", data_ym1);
            p.Add("data_ym2", data_ym2);
            p.Add("grp_no", grp_no);
            p.Add("inid", inid);

            if (isPaging)
            {
                return DBWork.PagingQuery<FA0077>(sql, p, DBWork.Transaction);
            }
            else
            {
                return DBWork.Connection.Query<FA0077>(sql, p, DBWork.Transaction);
            }
        }

        public int DeleteAbsFeeByYm(string ym)
        {
            string sql = @"delete from MM_ABS_FEE
                            where data_ym = :ym";
            return DBWork.Connection.Execute(sql, new { ym }, DBWork.Transaction);
        }
        public int insertAbsFeeByYm(string ym)
        {
            string sql = @"insert into MM_ABS_FEE(data_ym, inid, abs_amount, create_date, create_user)
                           select data_ym, inid, sum(use_amount) as abs_amount,
                                  sysdate as create_date, '不計價衛材' as create_user
                             from (
                                    select b.data_ym, b.inid,
                                           (b.DISC_CPRICE * b.use_qty) as USE_AMOUNT
                                      from (
                                             select mmcode from MI_MAST
                                              where m_paykind = '3'
                                                and mat_class = '02'
                                           )A
                                    inner join
                                        (select DATA_YM,
                                                (select INID from MI_WHMAST where WH_NO=a.WH_NO) as INID,
                                                MMCODE,
                                                (select DISC_CPRICE from MI_WHCOST 
                                                  where DATA_YM=a.DATA_YM and MMCODE=a.MMCODE
                                                    and DATA_YM=SET_YM 
                                                ) as DISC_CPRICE,
                                                USE_QTY
                                           from MI_WINVMON a
                                          where 1=1
                                            and DATA_YM= :ym
                                            and (select 1 from MI_WHMAST where wh_no = a.wh_no and cancel_id = 'N') = 1
                                        ) B
                                    on A.mmcode = B.mmcode
                                  )
                            group by data_ym, inid
                           having data_ym is not null
                            order by data_ym, inid";

            return DBWork.Connection.Execute(sql, new { ym }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetT3Year()
        {
            string sql = @"
                select set_ym as value, set_ym as text
                  from (
                        select distinct substr(set_ym, 1 ,3) as set_ym
                          from MI_MNSET
                         where set_ym >= '10905'
                         order by set_ym desc
                  )
                  order by set_ym desc
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
        #endregion

    }
}