using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;

namespace WebApp.Repository.F
{

    public class FA0010_MODEL : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; }
        public string MAT_CLASS { get; set; }
        public string ITEM { get; set; }
        public string M01 { get; set; }
        public string M02 { get; set; }
        public string M03 { get; set; }
        public string M04 { get; set; }
        public string M05 { get; set; }
        public string M06 { get; set; }
        public string M07 { get; set; }
        public string M08 { get; set; }
        public string M09 { get; set; }
        public string M10 { get; set; }
        public string M11 { get; set; }
        public string M12 { get; set; }
        public string MAVG { get; set; }
        public string AVGINV_RATIO { get; set; }
        public string AVGDAYS { get; set; }
        public string rep_time { get; set; }

    }
    public class FA0010Repository : JCLib.Mvc.BaseRepository
    {

        public FA0010Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0010_MODEL> GetAll( string mysysdate,int page_index, int page_size)
        {
            var p = new DynamicParameters();

            var sql = @"select '01a' as seq,'藥品類' as mat_class,'期初金額' as item,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='01'),0),2) as m01,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time  and mat_class='01' and mon='02'),0),2) as m02,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='03'),0),2) as m03,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='04'),0),2) as m04,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='05'),0),2) as m05,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='06'),0),2) as m06,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='07'),0),2) as m07,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='08'),0),2) as m08,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='09'),0),2) as m09,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='10'),0),2) as m10,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='11'),0),2) as m11,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='12'),0),2) as m12,
ROUND((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time  and mat_class='01')/12,2) as mavg,
null as avginv_ratio,null as avgdays from dual 
    union select '01b' as seq,'' as mat_class,'期末金額' as item,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='01'),0),2) as m01,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time   and mat_class='01' and mon='02'),0),2) as m02,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='03'),0),2) as m03,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='04'),0),2) as m04,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='05'),0),2) as m05,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='06'),0),2) as m06,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='07'),0),2) as m07,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='08'),0),2) as m08,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='09'),0),2) as m09,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='10'),0),2) as m10,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='11'),0),2) as m11,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='12'),0),2) as m12,
    ROUND((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01')/12,2) as mavg,
    null as avginv_ratio,null as avgdays from dual
	union select '01c' as seq,'' as mat_class,'耗用金額' as item,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='01'),0),2) as m01,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='02'),0),2) as m02,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='03'),0),2) as m03,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='04'),0),2) as m04,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='05'),0),2) as m05,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='06'),0),2) as m06,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='07'),0),2) as m07,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='08'),0),2) as m08,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='09'),0),2) as m09,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='10'),0),2) as m10,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='11'),0),2) as m11,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='12'),0),2) as m12,
	ROUND((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01')/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '01d' as seq,'' as mat_class,'平均庫存金額' as item,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='01'),0),2) as m01,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='02'),0),2) as m02,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='03'),0),2) as m03,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='04'),0),2) as m04,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='05'),0),2) as m05,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='06'),0),2) as m06,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='07'),0),2) as m07,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='08'),0),2) as m08,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='09'),0),2) as m09,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='10'),0),2) as m10,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='11'),0),2) as m11,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='12'),0),2) as m12,
	ROUND((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01')/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '01e' as seq,'' as mat_class,'期末比值' as item,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='01'),0),2) as m01,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='02'),0),2) as m02,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='03'),0),2) as m03,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='04'),0),2) as m04,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='05'),0),2) as m05,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='06'),0),2) as m06,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='07'),0),2) as m07,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='08'),0),2) as m08,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='09'),0),2) as m09,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='10'),0),2) as m10,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='11'),0),2) as m11,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='12'),0),2) as m12,
	ROUND((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01'),2) as mavg,
	ROUND((select sum(use_amt)/sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01'),2) as avginv_ratio,
	ROUND(365/(select sum(use_amt)/sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01'),2) as avgdays from dual
    union select '02a' as seq,'消耗性醫療器材類' as mat_class,'期初金額' as item,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='01'),0),2) as m01,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='02'),0),2) as m02,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='03'),0),2) as m03,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='04'),0),2) as m04,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='05'),0),2) as m05,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='06'),0),2) as m06,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='07'),0),2) as m07,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='08'),0),2) as m08,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='09'),0),2) as m09,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='10'),0),2) as m10,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='11'),0),2) as m11,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='12'),0),2) as m12,
	ROUND((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02')/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '02b' as seq,'' as mat_class,'期末金額' as item,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='01'),0),2) as m01,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='02'),0),2) as m02,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='03'),0),2) as m03,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='04'),0),2) as m04,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='05'),0),2) as m05,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='06'),0),2) as m06,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='07'),0),2) as m07,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='08'),0),2) as m08,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='09'),0),2) as m09,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='10'),0),2) as m10,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='11'),0),2) as m11,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='12'),0),2) as m12,
	ROUND((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02')/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '02c' as seq,'' as mat_class,'耗用金額' as item,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='01'),0),2) as m01,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='02'),0),2) as m02,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='03'),0),2) as m03,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='04'),0),2) as m04,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='05'),0),2) as m05,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='06'),0),2) as m06,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='07'),0),2) as m07,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='08'),0),2) as m08,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='09'),0),2) as m09,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='10'),0),2) as m10,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='11'),0),2) as m11,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='12'),0),2) as m12,
	ROUND((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02')/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '02d' as seq,'' as mat_class,'平均庫存金額' as item,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='01'),0),2) as m01,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='02'),0),2) as m02,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='03'),0),2) as m03,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='04'),0),2) as m04,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='05'),0),2) as m05,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='06'),0),2) as m06,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='07'),0),2) as m07,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='08'),0),2) as m08,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='09'),0),2) as m09,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='10'),0),2) as m10,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='11'),0),2) as m11,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='12'),0),2) as m12,
	ROUND((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02')/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '02e' as seq,'' as mat_class,'期末比值' as item,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='01'),0),2) as m01,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='02'),0),2) as m02,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='03'),0),2) as m03,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='04'),0),2) as m04,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='05'),0),2) as m05,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='06'),0),2) as m06,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='07'),0),2) as m07,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='08'),0),2) as m08,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='09'),0),2) as m09,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='10'),0),2) as m10,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='11'),0),2) as m11,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='12'),0),2) as m12,
	ROUND((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02'),2) as mavg,
	ROUND((select sum(use_amt)/sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02'),2) as avginv_ratio,
	ROUND(365/(select sum(use_amt)/sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02'),2) as avgdays from dual
    union select '03a' as seq,'藥品醫材合計' as mat_class,'期初金額' as item,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='01'),0),2) as m01,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='02'),0),2) as m02,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='03'),0),2) as m03,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='04'),0),2) as m04,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='05'),0),2) as m05,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='06'),0),2) as m06,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='07'),0),2) as m07,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='08'),0),2) as m08,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='09'),0),2) as m09,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='10'),0),2) as m10,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='11'),0),2) as m11,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='12'),0),2) as m12,
	ROUND((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time)/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '03b' as seq,'' as mat_class,'期末金額' as item,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='01'),0),2) as m01,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='02'),0),2) as m02,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='03'),0),2) as m03,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='04'),0),2) as m04,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='05'),0),2) as m05,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='06'),0),2) as m06,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='07'),0),2) as m07,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='08'),0),2) as m08,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='09'),0),2) as m09,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='10'),0),2) as m10,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='11'),0),2) as m11,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='12'),0),2) as m12,
	ROUND((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time)/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '03c' as seq,'' as mat_class,'耗用金額' as item,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='01'),0),2) as m01,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='02'),0),2) as m02,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='03'),0),2) as m03,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='04'),0),2) as m04,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='05'),0),2) as m05,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='06'),0),2) as m06,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='07'),0),2) as m07,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='08'),0),2) as m08,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='09'),0),2) as m09,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='10'),0),2) as m10,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='11'),0),2) as m11,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='12'),0),2) as m12,
	ROUND((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time)/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '03d' as seq,'' as mat_class,'平均庫存金額' as item,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='01'),0),2) as m01,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='02'),0),2) as m02,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='03'),0),2) as m03,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='04'),0),2) as m04,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='05'),0),2) as m05,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='06'),0),2) as m06,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='07'),0),2) as m07,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='08'),0),2) as m08,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='09'),0),2) as m09,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='10'),0),2) as m10,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='11'),0),2) as m11,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='12'),0),2) as m12,
	ROUND((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time)/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '03e' as seq,'' as mat_class,'期末比值' as item,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='01'),0),2) as m01,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='02'),0),2) as m02,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='03'),0),2) as m03,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='04'),0),2) as m04,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='05'),0),2) as m05,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='06'),0),2) as m06,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='07'),0),2) as m07,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='08'),0),2) as m08,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='09'),0),2) as m09,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='10'),0),2) as m10,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='11'),0),2) as m11,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='12'),0),2) as m12,
	ROUND((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time),2) as mavg,
	ROUND((select sum(use_amt)/sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time),2) as avginv_ratio,
	ROUND(365/(select sum(use_amt)/sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time),2) as avgdays from dual";



            p.Add(":rep_time", mysysdate);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0010_MODEL>(GetPagingStatement(sql), p, DBWork.Transaction);
        }

        public IEnumerable<FA0010_MODEL> GetPrintData(string mysysdate)
        {
            var p = new DynamicParameters();

            var sql = @"select '01a' as seq,'藥' as mat_class,'期初金額' as item,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='01'),0),2) as m01,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time  and mat_class='01' and mon='02'),0),2) as m02,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='03'),0),2) as m03,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='04'),0),2) as m04,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='05'),0),2) as m05,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='06'),0),2) as m06,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='07'),0),2) as m07,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='08'),0),2) as m08,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='09'),0),2) as m09,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='10'),0),2) as m10,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='11'),0),2) as m11,
ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='12'),0),2) as m12,
ROUND((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time  and mat_class='01')/12,2) as mavg,
null as avginv_ratio,null as avgdays from dual 
    union select '01b' as seq,'品' as mat_class,'期末金額' as item,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='01'),0),2) as m01,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time   and mat_class='01' and mon='02'),0),2) as m02,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='03'),0),2) as m03,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='04'),0),2) as m04,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='05'),0),2) as m05,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='06'),0),2) as m06,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='07'),0),2) as m07,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='08'),0),2) as m08,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='09'),0),2) as m09,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='10'),0),2) as m10,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='11'),0),2) as m11,
    ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01' and mon='12'),0),2) as m12,
    ROUND((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')= :rep_time and mat_class='01')/12,2) as mavg,
    null as avginv_ratio,null as avgdays from dual
	union select '01c' as seq,'類' as mat_class,'耗用金額' as item,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='01'),0),2) as m01,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='02'),0),2) as m02,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='03'),0),2) as m03,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='04'),0),2) as m04,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='05'),0),2) as m05,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='06'),0),2) as m06,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='07'),0),2) as m07,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='08'),0),2) as m08,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='09'),0),2) as m09,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='10'),0),2) as m10,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='11'),0),2) as m11,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='12'),0),2) as m12,
	ROUND((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01')/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '01d' as seq,'' as mat_class,'平均庫存金額' as item,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='01'),0),2) as m01,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='02'),0),2) as m02,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='03'),0),2) as m03,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='04'),0),2) as m04,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='05'),0),2) as m05,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='06'),0),2) as m06,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='07'),0),2) as m07,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='08'),0),2) as m08,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='09'),0),2) as m09,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='10'),0),2) as m10,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='11'),0),2) as m11,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='12'),0),2) as m12,
	ROUND((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01')/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '01e' as seq,'' as mat_class,'期末比值' as item,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='01'),0),2) as m01,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='02'),0),2) as m02,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='03'),0),2) as m03,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='04'),0),2) as m04,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='05'),0),2) as m05,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='06'),0),2) as m06,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='07'),0),2) as m07,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='08'),0),2) as m08,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='09'),0),2) as m09,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='10'),0),2) as m10,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='11'),0),2) as m11,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01' and mon='12'),0),2) as m12,
	ROUND((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01'),2) as mavg,
	ROUND((select sum(use_amt)/sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01'),2) as avginv_ratio,
	ROUND(365/(select sum(use_amt)/sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='01'),2) as avgdays from dual
    union select '02a' as seq,'消耗' as mat_class,'期初金額' as item,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='01'),0),2) as m01,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='02'),0),2) as m02,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='03'),0),2) as m03,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='04'),0),2) as m04,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='05'),0),2) as m05,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='06'),0),2) as m06,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='07'),0),2) as m07,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='08'),0),2) as m08,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='09'),0),2) as m09,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='10'),0),2) as m10,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='11'),0),2) as m11,
	ROUND(NVL((select p_inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='12'),0),2) as m12,
	ROUND((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02')/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '02b' as seq,'性醫' as mat_class,'期末金額' as item,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='01'),0),2) as m01,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='02'),0),2) as m02,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='03'),0),2) as m03,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='04'),0),2) as m04,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='05'),0),2) as m05,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='06'),0),2) as m06,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='07'),0),2) as m07,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='08'),0),2) as m08,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='09'),0),2) as m09,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='10'),0),2) as m10,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='11'),0),2) as m11,
	ROUND(NVL((select inv_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='12'),0),2) as m12,
	ROUND((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02')/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '02c' as seq,'療器' as mat_class,'耗用金額' as item,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='01'),0),2) as m01,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='02'),0),2) as m02,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='03'),0),2) as m03,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='04'),0),2) as m04,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='05'),0),2) as m05,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='06'),0),2) as m06,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='07'),0),2) as m07,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='08'),0),2) as m08,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='09'),0),2) as m09,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='10'),0),2) as m10,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='11'),0),2) as m11,
	ROUND(NVL((select use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='12'),0),2) as m12,
	ROUND((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02')/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '02d' as seq,'材類' as mat_class,'平均庫存金額' as item,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='01'),0),2) as m01,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='02'),0),2) as m02,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='03'),0),2) as m03,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='04'),0),2) as m04,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='05'),0),2) as m05,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='06'),0),2) as m06,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='07'),0),2) as m07,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='08'),0),2) as m08,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='09'),0),2) as m09,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='10'),0),2) as m10,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='11'),0),2) as m11,
	ROUND(NVL((select (p_inv_amt+inv_amt)/2 from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='12'),0),2) as m12,
	ROUND((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02')/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '02e' as seq,'' as mat_class,'期末比值' as item,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='01'),0),2) as m01,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='02'),0),2) as m02,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='03'),0),2) as m03,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='04'),0),2) as m04,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='05'),0),2) as m05,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='06'),0),2) as m06,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='07'),0),2) as m07,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='08'),0),2) as m08,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='09'),0),2) as m09,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='10'),0),2) as m10,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='11'),0),2) as m11,
	ROUND(NVL((select inv_amt/use_amt from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02' and mon='12'),0),2) as m12,
	ROUND((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02'),2) as mavg,
	ROUND((select sum(use_amt)/sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02'),2) as avginv_ratio,
	ROUND(365/(select sum(use_amt)/sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mat_class='02'),2) as avgdays from dual
    union select '03a' as seq,'藥品' as mat_class,'期初金額' as item,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='01'),0),2) as m01,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='02'),0),2) as m02,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='03'),0),2) as m03,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='04'),0),2) as m04,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='05'),0),2) as m05,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='06'),0),2) as m06,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='07'),0),2) as m07,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='08'),0),2) as m08,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='09'),0),2) as m09,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='10'),0),2) as m10,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='11'),0),2) as m11,
	ROUND(NVL((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='12'),0),2) as m12,
	ROUND((select sum(p_inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time)/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '03b' as seq,'醫材' as mat_class,'期末金額' as item,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='01'),0),2) as m01,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='02'),0),2) as m02,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='03'),0),2) as m03,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='04'),0),2) as m04,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='05'),0),2) as m05,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='06'),0),2) as m06,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='07'),0),2) as m07,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='08'),0),2) as m08,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='09'),0),2) as m09,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='10'),0),2) as m10,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='11'),0),2) as m11,
	ROUND(NVL((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='12'),0),2) as m12,
	ROUND((select sum(inv_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time)/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '03c' as seq,'合計' as mat_class,'耗用金額' as item,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='01'),0),2) as m01,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='02'),0),2) as m02,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='03'),0),2) as m03,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='04'),0),2) as m04,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='05'),0),2) as m05,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='06'),0),2) as m06,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='07'),0),2) as m07,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='08'),0),2) as m08,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='09'),0),2) as m09,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='10'),0),2) as m10,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='11'),0),2) as m11,
	ROUND(NVL((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='12'),0),2) as m12,
	ROUND((select sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time)/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '03d' as seq,'' as mat_class,'平均庫存金額' as item,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='01'),0),2) as m01,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='02'),0),2) as m02,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='03'),0),2) as m03,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='04'),0),2) as m04,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='05'),0),2) as m05,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='06'),0),2) as m06,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='07'),0),2) as m07,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='08'),0),2) as m08,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='09'),0),2) as m09,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='10'),0),2) as m10,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='11'),0),2) as m11,
	ROUND(NVL((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='12'),0),2) as m12,
	ROUND((select sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time)/12,2) as mavg,
	null as avginv_ratio,null as avgdays from dual
    union select '03e' as seq,'' as mat_class,'期末比值' as item,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='01'),0),2) as m01,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='02'),0),2) as m02,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='03'),0),2) as m03,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='04'),0),2) as m04,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='05'),0),2) as m05,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='06'),0),2) as m06,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='07'),0),2) as m07,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='08'),0),2) as m08,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='09'),0),2) as m09,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='10'),0),2) as m10,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='11'),0),2) as m11,
	ROUND(NVL((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time and mon='12'),0),2) as m12,
	ROUND((select sum(inv_amt)/sum(use_amt) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time),2) as mavg,
	ROUND((select sum(use_amt)/sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time),2) as avginv_ratio,
	ROUND(365/(select sum(use_amt)/sum((p_inv_amt+inv_amt)/2) from TMP_INVCTL_YR_RP where to_char(rep_time,'yyyymmddhh24mmss')=:rep_time),2) as avgdays from dual";



            p.Add(":rep_time", mysysdate);


            return DBWork.Connection.Query<FA0010_MODEL>(sql, p, DBWork.Transaction);
        }

        public int Create(string mysysdate, string year)
        {
            var p = new DynamicParameters();

            var sql = @"insert into TMP_INVCTL_YR_RP 
            (rep_time,rep_yr,mat_class,mon,p_inv_amt,inv_amt,use_amt, lowturn_inv_amt) 
     select To_Date(:RAWSYSDATE,'YYYYMMDD HH24:MI:SS') as rep_time, '108' as rep_yr,B.MAT_CLASS, SUBSTR(A.DATA_YM,4,2) MON, 
        SUM(ROUND(A.P_INV_QTY *A.PMN_AVGPRICE,2)) P_INV_AMT, 
        SUM(ROUND(A.INV_QTY*A.AVG_PRICE,2)) INV_AMT, 
        SUM(ROUND(A.OUT_QTY*A.AVG_PRICE,2)) USE_AMT, 
        SUM(DECODE(A.OUT_QTY,0,ROUND(A.INV_QTY*A.AVG_PRICE,2),0)) LOWTURN_INV_AMT 
     FROM V_COST_ALL4 A, MI_MAST B 
    WHERE A.MMCODE=B.MMCODE AND SUBSTR(A.DATA_YM,1,3) = :MYYEAR 
      AND B.MAT_CLASS IN ('01','02') 
  GROUP BY B.MAT_CLASS,SUBSTR(A.DATA_YM,4,2)";

            p.Add(":RAWSYSDATE", mysysdate);
            p.Add(":MYYEAR", year);

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int Delete()
        {
            // 刪除今天以前的資料
            var sql = @"DELETE TMP_INVCTL_YR_RP where rep_time<trunc(sysdate)";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public String GetSYSDATE()
        {
            string sql = @"select to_char(sysdate,　'yyyymmddhh24mmss') from dual";
            return DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
        }
        public String GetRawSYSDATE()
        {
            string sql = @"select sysdate as rep_time from dual";
            return DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
        }

        public IEnumerable<ComboModel> GetSt_YEAR()
        {
            string sql = @"SELECT distinct substr(SET_YM,1,3) as VALUE, " +
                "substr(SET_YM,1,3) as COMBITEM from MI_MNSET WHERE SET_STATUS='C'";
            return DBWork.Connection.Query<ComboModel>(sql, DBWork.Transaction);
        }
    }
}