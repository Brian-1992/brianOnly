using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AB0038 : JCLib.Mvc.BaseModel
    {
        public string CHECK_FLAG { get; set; } //比對是否通過
        public string CHECK_MSG { get; set; } //比對訊息
        public string LOAD_MSG { get; set; } //比對訊息

        // HIS_BASORDD
        public string AGENTNAME { get; set; } //申請商名稱
        public string ARMYINSUAMOUNT { get; set; } //軍聯標單價
        public string ARMYINSUORDERCODE { get; set; } //軍聯標健保碼
        public string ATTACHTRANSQTYI { get; set; } //住院申報轉換量
        public string ATTACHTRANSQTYO { get; set; } //門診申報轉換量
        public string CASEFROM { get; set; } //標案來源
        public string CONTRACNO { get; set; } //合約碼
        public string CONTRACTPRICE { get; set; } //合約單價
        public string COSTAMOUNT { get; set; } //進價
        public string DENTALREFFLAG { get; set; } //牙科轉診加成
        public string DRUGCASEFROM { get; set; } //藥品標案來源
        public string ENDDATETIME { get; set; } //結束日期時間
        public string EXAMINEDISCFLAG { get; set; } //檢查折扣否
        public string EXECFLAG { get; set; } //執行否
        public string HOSPEMGFLAG { get; set; } //自費急件加成
        public string HOSPKIDFLAG { get; set; } //自費兒童加成
        public string INSUAMOUNT1 { get; set; } //健保價
        public string INSUAMOUNT2 { get; set; } //健保點數二
        public string INSUEMGFLAG { get; set; } //健保急件加成
        public string INSUKIDFLAG { get; set; } //兒童加成否
        public string INSUORDERCODE { get; set; } //健保碼
        public string INSUSIGNI { get; set; } //健保負擔碼(住院)
        public string INSUSIGNO { get; set; } //健保負擔碼(門診)
        public string MAMAGEFLAG { get; set; } //是否優惠
        public string MAMAGERATE { get; set; } //優惠%
        public string ORIGINALPRODUCER { get; set; } //製造廠名稱
        public string PAYAMOUNT1 { get; set; } //自費價
        public string PAYAMOUNT2 { get; set; } //自付金額2
        public string PPFPERCENTAGE { get; set; } //提成百分比
        public string PPFTYPE { get; set; } //提成類別碼
        public string PTRESOLUTIONCLASS { get; set; } //藥審會決議合約碼
        public string STOCKTRANSQTYI { get; set; } //住院扣庫轉換量
        public string STOCKTRANSQTYO { get; set; } //門診扣庫轉換量
        public string SUPPLYNO { get; set; } //廠商代碼

        // HIS_BASORDM
        public string AGGREGATECODE { get; set; } //累計用藥
        public string AHFSCODE1 { get; set; } //藥品成份1
        public string AHFSCODE2 { get; set; } //藥品成份2
        public string AHFSCODE3 { get; set; } //藥品成份3
        public string AHFSCODE4 { get; set; } //藥品成份4
        public string AIRDELIVERY { get; set; } //可氣送
        public string ANTIBIOTICSCODE { get; set; } //抗生素等級
        public string APPENDMATERIALFLAG { get; set; } //健保帶材料否
        public string ATCCODE1 { get; set; } //藥理分類 ATCCODE1
        public string ATCCODE2 { get; set; } //藥理分類 ATCCODE2
        public string ATCCODE3 { get; set; } //藥理分類 ATCCODE3
        public string ATCCODE4 { get; set; } //藥理分類 ATCCODE4
        public string ATTACHUNIT { get; set; } //申報(計價)單位
        public string BATCHNOFLAG { get; set; } //需有批號效期品項
        public string BIOLOGICALAGENT { get; set; } //生物製劑
        public string BLOODPRODUCT { get; set; } //血液製劑
        public string BUYORDERFLAG { get; set; } //買斷藥
        public string CARRYKINDI { get; set; } //住院消耗歸整
        public string CARRYKINDO { get; set; } //門急消耗歸整
        public string CHANGEABLEFLAG { get; set; } //可線上異動否
        public string CHECKINSWITCH { get; set; } //是否需報到
        public string COLDSTORAGEFLAG { get; set; } //冷藏存放
        public string COSTEXCLUDECLASS { get; set; } //成本排除特殊類別
        public string CURETYPE { get; set; } //特殊治療種類
        public string DANGERDRUGFLAG { get; set; } //高警訊藥品
        public string DANGERDRUGMEMO { get; set; } //高警訊藥品提示
        public string DCL { get; set; } //事前專審藥品替換群組
        public string DOHLICENSENO { get; set; } //衛生署核准字號
        public string DOSE { get; set; } //預設劑量
        public string DRUGELEMCODE1 { get; set; } //藥品成份1
        public string DRUGELEMCODE2 { get; set; } //藥品成份2
        public string DRUGELEMCODE3 { get; set; } //藥品成份3
        public string DRUGELEMCODE4 { get; set; } //藥品成份4
        public string DRUGHOSPBEGINDATE { get; set; } //藥品契約生效起日
        public string DRUGHOSPENDDATE { get; set; } //藥品契約生效迄日
        public string DRUGPARENTCODE1 { get; set; } //成份母層代碼1
        public string DRUGPARENTCODE2 { get; set; } //成份母層代碼2
        public string DRUGPARENTCODE3 { get; set; } //成份母層代碼3
        public string DRUGPARENTCODE4 { get; set; } //成份母層代碼4
        public string EXAMINEDRUGFLAG { get; set; } //檢驗用藥
        public string EXCEPTIONCODE { get; set; } //例外備註碼
        public string EXCEPTIONFLAG { get; set; } //是否有例外備註
        public string EXCLUDEFLAG { get; set; } //除外項目
        public string EXORDERFLAG { get; set; } //特殊品項
        public string FIXDOSEFLAG { get; set; } //預設劑量
        public string FIXPATHNOFLAG { get; set; } //限制途徑
        public string FREEZING { get; set; } //是否需冷凍
        public string FREQNOI { get; set; } //住院給藥頻率
        public string FREQNOO { get; set; } //門診給藥頻率
        public string GERIATRIC { get; set; } //老年人劑量調整
        public string HEPATITISCODE { get; set; } //BC肝用藥註記
        public string HIGHPRICEFLAG { get; set; } //高價用藥
        public string HOSPCHARGEID1 { get; set; } //院內費用類別1(院內歸屬)
        public string HOSPCHARGEID2 { get; set; } //院內費用類別2(院內歸屬)
        public string HOSPEXAMINEFLAG { get; set; } //內審用藥
        public string HOSPEXAMINEQTYFLAG { get; set; } //內審限制用量
        public string INPDISPLAYFLAG { get; set; } //住院醫囑顯示
        public string INSUCHARGEID1 { get; set; } //健保費用類別1(健保歸屬)
        public string INSUCHARGEID2 { get; set; } //健保費用類別2(健保歸屬)
        public string INSUOFFERFLAG { get; set; } //保險給付否(Y/N)
        public string ISCURECODE { get; set; } //同療程項目
        public string LIGHTAVOIDFLAG { get; set; } //避光存放
        public string LIMITEDQTYI { get; set; } //住院倍數核發
        public string LIMITEDQTYO { get; set; } //門診倍數核發
        public string LIMITFLAG { get; set; } //開立限制
        public string LIVERLIMITED { get; set; } //肝功能不良需調整劑量
        public string MACHINEFLAG { get; set; } //藥包機品項
        public string MAINCUREITEM { get; set; } //特定治療項目
        public string MAXDAYSI { get; set; } //住院限制開立日數
        public string MAXDAYSO { get; set; } //門診限制開立日數
        public string MAXQTYI { get; set; } //住院限制開立數量
        public string MAXQTYO { get; set; } //門診限制開立數量
        public string MAXQTYPERDAY { get; set; } //一日極量
        public string MAXQTYPERTIME { get; set; } //一次極量
        public string MAXTAKETIMES { get; set; } //限制次數
        public string MILITARYEXCLUDECLASS { get; set; } //軍醫局排除類別
        public string NEEDOPDTYPEFLAG { get; set; } //處置需報調劑方式
        public string NEEDREGIONFLAG { get; set; } //處置需報部位
        public string ONLYROUNDFLAG { get; set; } //不可剝半
        public string OPENDATE { get; set; } //系統啟用日期
        public string OPERATIONFLAG { get; set; } //手術碼
        public string ORDERABLEDRUGFORM { get; set; } //藥品劑型規格(電子簽章)
        public string ORDERCHINNAME { get; set; } //中文名稱
        public string ORDERCHINUNIT { get; set; } //中文單位
        public string ORDERCODE { get; set; } //院內代碼
        public string ORDERCODESORT { get; set; } //醫令排序
        public string ORDERCONDCODE { get; set; } //給付條文代碼
        public string ORDERDAYS { get; set; } //預設開立天數
        public string ORDERDCFLAG { get; set; } //停用碼
        public string ORDEREASYNAME { get; set; } //簡稱
        public string ORDERENGNAME { get; set; } //英文名稱
        public string ORDERHOSPNAME { get; set; } //院內名稱
        public string ORDERKIND { get; set; } //醫令類別(申報定義)
        public string ORDERUNIT { get; set; } //醫囑單位
        public string ORDERUSETYPE { get; set; } //醫令使用狀態
        public string PATHNO { get; set; } //院內給藥途徑(部位)代碼
        public string PUBLICDRUGFLAG { get; set; } //公藥否(Y/N)
        public string RAREDISORDERFLAG { get; set; } //罕見疾病用藥
        public string RAYPOSITION { get; set; } //放射部位
        public string RENALLIMITED { get; set; } //腎功能不良需調整劑量
        public string REPORTFLAG { get; set; } //是否發報告
        public string RESEARCHDRUGFLAG { get; set; } //研究用藥
        public string RESTRICTCODE { get; set; } //管制用藥
        public string RESTRICTTYPE { get; set; } //限制狀態
        public string RETURNDRUGFLAG { get; set; } //合理回流藥
        public string RFIDCODE { get; set; } //RFID條碼
        public string SAFETYSYRINGE { get; set; } //安全針具(衛材)
        public string SCIENTIFICNAME { get; set; } //成份名稱
        public string SECTIONNO { get; set; } //院內科別代碼
        public string SENDUNITFLAG { get; set; } //檢查類別否
        public string SIGNFLAG { get; set; } //是否需 Sign In
        public string SINGLEITEMFLAG { get; set; } //獨立處方箋
        public string SOONCULLFLAG { get; set; } //開立醫令即為報到
        public string SPECIALORDERKIND { get; set; } //專案碼
        public string STOCKUNIT { get; set; } //扣庫單位
        public string SUBSTITUTE1 { get; set; } //替代院內代碼1
        public string SUBSTITUTE2 { get; set; } //替代院內代碼2
        public string SUBSTITUTE3 { get; set; } //替代院內代碼3
        public string SUBSTITUTE4 { get; set; } //替代院內代碼4
        public string SUBSTITUTE5 { get; set; } //替代院內代碼5
        public string SUBSTITUTE6 { get; set; } //替代院內代碼6
        public string SUBSTITUTE7 { get; set; } //替代院內代碼7
        public string SUBSTITUTE8 { get; set; } //替代院內代碼8
        public string SYMPTOMCHIN { get; set; } //適應症(中文)
        public string SYMPTOMENG { get; set; } //適應症(英文)
        public string TAKEKIND { get; set; } //服用藥別
        public string TDMFLAG { get; set; } //TDM 藥品
        public string TRANSCOMPUTEFLAG { get; set; } //結轉計價
        public string UDPOWDERFLAG { get; set; } //UD磨粉
        public string UDSERVICEFLAG { get; set; } //UD服務(Y/N)
        public string UNABLEPOWDERFLAG { get; set; } //不可磨粉
        public string VACCINE { get; set; } //疫苗註記
        public string VACCINECLASS { get; set; } //疫苗類別
        public string VALIDDAYSI { get; set; } //住院效期日數
        public string VALIDDAYSO { get; set; } //門診效期日數
        public string WEIGHTTYPE { get; set; } //體重及安全量：計算別
        public string WEIGHTUNITLIMIT { get; set; } //體重及安全量：限制數量
        public string ZEROCASESTATE { get; set; } //零購品結案註記

        // HIS_STKDMIT
        public string APPLYTRANSQTY { get; set; } //單位轉換量(院內/最小)
        public string APPLYUNIT { get; set; } //院內單位(預設扣庫單位)
        public string ARMYORDERCODE { get; set; } //軍品院內碼 民品所對應的軍品院內碼
        public string CHINATTENTION { get; set; } //注意事項(中文)
        public string CHINSIDEEFFECT { get; set; } //主要副作用(中文)
        public string CLASSIFIEDARMYNO { get; set; } //軍聯項次組別
        public string COMMITTEECODE { get; set; } //藥委會品項
        public string COMMITTEEMEMO { get; set; } //藥委會註記
        public string COMPONENTNUNIT { get; set; } //成份量及單位
        public string COMPONENTNUNIT2 { get; set; } //成份量及單位2
        public string COMPONENTNUNIT3 { get; set; } //成份量及單位3
        public string COMPONENTNUNIT4 { get; set; } //成份量及單位4
        public string CONTRACTEFFECTIVEDATE { get; set; } //合約效期
        public string DANGERBEGIN { get; set; } //TDM 危急值 起
        public string DANGEREND { get; set; } //TDM 危急值 迄
        public string DOHSYMPTOM { get; set; } //衛生署核准適應症
        public string DRUGAPPLYTYPE { get; set; } //藥品請領類別
        public string DRUGCLASS { get; set; } //藥品類別
        public string DRUGCLASSIFY { get; set; } //藥品性質欄位
        public string DRUGENGEXTERIOR { get; set; } //藥品外觀(英文)
        public string DRUGEXTERIOR { get; set; } //藥品外觀
        public string DRUGFORM { get; set; } //藥品劑型
        public string DRUGLEAFLETLINK { get; set; } //藥品仿單檔名
        public string DRUGMEMO { get; set; } //處方集
        public string DRUGPACKAGE { get; set; } //藥品包裝
        public string DRUGPICTURELINK { get; set; } //藥品圖片檔名
        public string DRUGTOTALAMOUNT { get; set; } //體積量
        public string DRUGTOTALAMOUNTUNIT { get; set; } //體積單位
        public string ENGATTENTION { get; set; } //注意事項(英文)
        public string ENGSIDEEFFECT { get; set; } //主要副作用(英文)
        public string FDASYMPTOM { get; set; } //FDA核准適應症
        public string GROUPARMYNO { get; set; } //軍聯項次分類
        public string INVENTORYFLAG { get; set; } //盤點品項(Y/N)
        public string ITEMARMYNO { get; set; } //軍聯項次號
        public string MANUFACTURER { get; set; } //原製造商(廠牌)
        public string MAXCURECONSISTENCY { get; set; } //TDM 合理治療濃度上限
        public string MINCURECONSISTENCY { get; set; } //TDM 合理治療濃度下限
        public string MULTIPRESCRIPTIONCODE { get; set; } //藥品單複方
        public string PARENTCODE { get; set; } //母藥註記
        public string PARENTORDERCODE { get; set; } //母藥院內碼
        public string PEARBEGIN { get; set; } //TDM 合理PEAK起
        public string PEAREND { get; set; } //TDM 合理PEAK迄
        public string PREGNANTGRADE { get; set; } //懷孕分級
        public string PURCHASECASETYPE { get; set; } //藥品採購案別
        public string PURCHASETRANSQTY { get; set; } //單位轉換量(採購/院內)
        public string PURCHASEUNIT { get; set; } //採購單位
        public string SONTRANSQTY { get; set; } //子藥轉換量
        public string SPECNUNIT { get; set; } //規格量及單位
        public string STOCKSOURCECODE { get; set; } //來源代碼
        public string STOCKUSECODE { get; set; } //扣庫規則分類
        public string SUCKLESECURITY { get; set; } //授乳安全性
        public string TDMMEMO1 { get; set; } //TDM 備註1
        public string TDMMEMO2 { get; set; } //TDM 備註2
        public string TDMMEMO3 { get; set; } //TDM 備註3
        public string TROUGHBEGIN { get; set; } //TDM 合理 Trough 起
        public string TROUGHEND { get; set; } //TDM 合理 Trough 迄
        public string WARN { get; set; } //警語
        public string YEARARMYNO { get; set; } //軍聯項次年號
    }
}
