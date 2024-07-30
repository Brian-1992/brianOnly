﻿using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AA0091M : JCLib.Mvc.BaseModel
    {
        public string ORDERCODE { get; set; }       //院內代碼
        public string ORDERENGNAME { get; set; }        //英文名稱
        public string ORDERCHINNAME { get; set; }       //中文名稱
        public string SCIENTIFICNAME { get; set; }      //成份名稱
        public string ORDERUNIT { get; set; }       //醫囑單位
        public string ORDERCHINUNIT { get; set; }       //中文單位
        public string ATTACHUNIT { get; set; }      //劑型單位
        public string STOCKUNIT { get; set; }       //扣庫單位
        public string SKORDERCODE { get; set; }         //藥品藥材代碼
        public string UDSERVICEFLAG { get; set; }       //UD服務
        public string TAKEKIND { get; set; }        //服用藥別
        public string LIMITEDQTYO { get; set; }         //門診倍數核發
        public string LIMITEDQTYI { get; set; }         //住院倍數核發
        public string BUYORDERFLAG { get; set; }        //買斷藥
        public string OPENDATE { get; set; }        //系統啟用日期
        public string PUBLICDRUGFLAG { get; set; }      //公藥否(Y/N)
        public string STARTDATE { get; set; }       //開始日期
        public string ORDERHOSPNAME { get; set; }       //院內名稱
        public string ORDEREASYNAME { get; set; }       //簡稱
        public string INSUOFFERFLAG { get; set; }       //保險給付否(Y/N)
        public string DCL { get; set; }         //DCL
        public string APPENDMATERIALFLAG { get; set; }      //健保帶材料否
        public string EXORDERFLAG { get; set; }         //特殊品項
        public string ORDERDCFLAG { get; set; }         //停用碼
        public string MAXQTYPERTIME { get; set; }       //一次極量
        public string MAXQTYPERDAY { get; set; }        //一日極量
        public string MAXTAKETIMES { get; set; }        //限制次數
        public string DOHLICENSENO { get; set; }        //衛生署核准字號
        public string RFIDCODE { get; set; }        //RFID條碼
        public string PATHNO { get; set; }      //給藥途徑(部位)代碼
        public string AGGREGATECODE { get; set; }       //累計用藥
        public string LIMITFLAG { get; set; }       //開立專用限制
        public string RESTRICTCODE { get; set; }        //管制用藥
        public string ANTIBIOTICSCODE { get; set; }         //抗生素等級
        public string CARRYKINDI { get; set; }      //住消耗歸整
        public string CARRYKINDO { get; set; }      //門急消耗歸整
        public string UDPOWDERFLAG { get; set; }        //UD磨粉
        public string RETURNDRUGFLAG { get; set; }      //合理回流藥
        public string RESEARCHDRUGFLAG { get; set; }        //研究用藥
        public string MACHINEFLAG { get; set; }         //藥包機品項
        public string TRANSCOMPUTEFLAG { get; set; }        //結轉計價
        public string FIXPATHNOFLAG { get; set; }       //限制途徑 Y/N
        public string SYMPTOMCHIN { get; set; }         //適應症(中文)
        public string SYMPTOMENG { get; set; }      //適應症(英文)
        public string ONLYROUNDFLAG { get; set; }       //不可剝半
        public string UNABLEPOWDERFLAG { get; set; }        //不可磨粉
        public string DANGERDRUGFLAG { get; set; }      //高警訊藥品
        public string DANGERDRUGMEMO { get; set; }      //高警訊藥品提示
        public string COLDSTORAGEFLAG { get; set; }         //冷藏存放
        public string LIGHTAVOIDFLAG { get; set; }      //避光存放
        public string CHANGESTATUS { get; set; }        //異動狀態
        public string FREQNOO { get; set; }         //門診給藥頻率
        public string FREQNOI { get; set; }         //住院給藥頻率
        public string ORDERDAYS { get; set; }       //預設開立天數
        public string DOSE { get; set; }        //劑量
        public string HOSPCHARGEID1 { get; set; }       //院內費用類別
        public string HOSPCHARGEID2 { get; set; }       //院內費用
        public string INSUCHARGEID1 { get; set; }       //健保費用類別
        public string INSUCHARGEID2 { get; set; }       //健保費用
        public string ORDERTYPE { get; set; }       //醫令類別
        public string ORDERKIND { get; set; }       //醫令類別(申報定義)
        public string HIGHPRICEFLAG { get; set; }       //高價用藥
        public string CURETYPE { get; set; }        //特殊治療種類
        public string INPDISPLAYFLAG { get; set; }      //住院醫囑顯示
        public string SOONCULLFLAG { get; set; }        //開立醫令即為報到
        public string SUBSTITUTE1 { get; set; }         //替代院內代碼1
        public string SUBSTITUTE2 { get; set; }         //替代院內代碼2
        public string SUBSTITUTE3 { get; set; }         //替代院內代碼3
        public string SUBSTITUTE4 { get; set; }         //替代院內代碼4
        public string SUBSTITUTE5 { get; set; }         //替代院內代碼5
        public string RELATEFLAGO { get; set; }         //連帶否(門診)
        public string RELATEFLAGI { get; set; }         //連帶否(住院)
        public string WEIGHTTYPE { get; set; }      //體重及安全量：計算別
        public string WEIGHTUNITLIMIT { get; set; }         //體重及安全量：限制數量
        public string RESTRICTTYPE { get; set; }        //限制狀態
        public string MAXQTYO { get; set; }         //門診限制開立數量
        public string MAXQTYI { get; set; }         //住院限制開立數量
        public string MAXDAYSO { get; set; }        //門診限制開立日數
        public string MAXDAYSI { get; set; }        //住院限制開立日數
        public string VALIDDAYSO { get; set; }      //門診效期日數
        public string VALIDDAYSI { get; set; }      //住院效期日數
        public string CHECKINSWITCH { get; set; }       //是否需報到
        public string REPORTFLAG { get; set; }      //是否發報告Y/N
        public string SINGLEITEMFLAG { get; set; }      //Single Item
        public string OPERATIONFLAG { get; set; }       //手術碼
        public string EXAMINEDRUGFLAG { get; set; }         //檢驗用藥
        public string MAINCUREITEM { get; set; }        //特定治療項目
        public string RAYPOSITION { get; set; }         //放射部位
        public string CCORDERCODE { get; set; }         //空針代碼
        public string XRAYORDERCODE { get; set; }       //放射第二張代碼
        public string ORDIAGCODE { get; set; }      //手術診斷碼
        public string ORDERCODESORT { get; set; }       //醫令排序
        public string SENDUNITFLAG { get; set; }        //傳送單位否
        public string SIGNFLAG { get; set; }        //是否需 Sign In
        public string EXCLUDEFLAG { get; set; }         //除外項目
        public string NEEDOPDTYPEFLAG { get; set; }         //處置需報調劑方式
        public string DRUGELEMCODE1 { get; set; }       //藥品成份Element1
        public string DRUGELEMCODE2 { get; set; }       //藥品成份Element2
        public string DRUGELEMCODE3 { get; set; }       //藥品成份Element3
        public string DRUGELEMCODE4 { get; set; }       //藥品成份Element4
        public string CHANGEABLEFLAG { get; set; }      //可線上異動否
        public string TDMFLAG { get; set; }         //TDM 藥品
        public string SPECIALORDERKIND { get; set; }        //專案碼
        public string NEEDREGIONFLAG { get; set; }      //處置需報部位
        public string ORDERUSETYPE { get; set; }        //醫令使用狀態
        public string FIXDOSEFLAG { get; set; }         //預設劑量
        public string RAREDISORDERFLAG { get; set; }        //罕見疾病用藥
        public string HOSPEXAMINEFLAG { get; set; }         //內審用藥
        public string ORDERCONDCODE { get; set; }       //給付條文代碼
        public string HOSPEXAMINEQTYFLAG { get; set; }      //內審限制用量
        public string CREATEDATETIME { get; set; }      //記錄建立日期/時間
        public string CREATEOPID { get; set; }      //記錄建立人員
        public string PROCDATETIME { get; set; }        //記錄處理日期/時間
        public string PROCOPID { get; set; }        //記錄處理人員
        public string HEPATITISCODE { get; set; }       //BC肝用藥註記
        //public string ORDERCODE { get; set; }       //院內代碼
        public string BEGINDATE { get; set; }       //生效起日
        public string ENDDATE { get; set; }         //生效迄日
        public string STOCKTRANSQTYO { get; set; }      //門診扣庫轉換量
        public string STOCKTRANSQTYI { get; set; }      //住院扣庫轉換量
        public string ATTACHTRANSQTYO { get; set; }         //門診劑型數量
        public string ATTACHTRANSQTYI { get; set; }         //住院劑型數量
        public string INSUAMOUNT1 { get; set; }         //健保點數一
        public string INSUAMOUNT2 { get; set; }         //健保點數二
        public string PAYAMOUNT1 { get; set; }      //自費點數一
        public string PAYAMOUNT2 { get; set; }      //自費點數二
        public string COSTAMOUNT { get; set; }      //進價
        public string MAMAGEFLAG { get; set; }      //是否收管理費
        public string MAMAGERATE { get; set; }      //管理費%
        public string INSUORDERCODE { get; set; }       //健保碼
        public string INSUSIGNI { get; set; }       //健保負擔碼(住院)
        public string INSUSIGNO { get; set; }       //健保負擔碼(門診)
        public string INSUEMGFLAG { get; set; }         //健保急件加成
        public string HOSPEMGFLAG { get; set; }         //自費急件加成
        public string DENTALREFFLAG { get; set; }       //牙科轉診加成
        public string PPFTYPE { get; set; }         //提成類別碼
        public string PPFPERCENTAGE { get; set; }       //提成百分比
        public string INSUKIDFLAG { get; set; }         //兒童加成否
        public string HOSPKIDFLAG { get; set; }         //自費兒童加成
        public string CONTRACTPRICE { get; set; }       //合約單價
        public string CONTRACNO { get; set; }       //合約碼
        public string SUPPLYNO { get; set; }        //廠商代碼(供應商代碼)
        public string CASEFROM { get; set; }        //標案來源
        public string EXAMINEDISCFLAG { get; set; }         //檢查折扣否
        public string ORIGINALPRODUCER { get; set; }        //製造廠名稱
        public string AGENTNAME { get; set; }       //申請商名稱
        //public string CREATEDATETIME { get; set; }      //記錄建立日期/時間
        //public string CREATEOPID { get; set; }      //記錄建立人員
        //public string PROCDATETIME { get; set; }        //記錄處理日期/時間
        //public string PROCOPID { get; set; }        //記錄處理人員
        public string ENDDATETIME { get; set; } 		//結束日期時間
    }
}
