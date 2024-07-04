Ext.define('WEBAPP.model.AB0038', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'CHECK_FLAG', type: 'string' }, //比對是否通過
        { name: 'CHECK_MSG', type: 'string' }, //比對訊息
        { name: 'LOAD_MSG', type: 'string' }, //載入訊息
        { name: 'AGENTNAME', type: 'string' }, //申請商名稱
        { name: 'ARMYINSUAMOUNT', type: 'string' }, //軍聯標單價
        { name: 'ARMYINSUORDERCODE', type: 'string' }, //軍聯標健保碼
        { name: 'ATTACHTRANSQTYI', type: 'string' }, //住院申報轉換量
        { name: 'ATTACHTRANSQTYO', type: 'string' }, //門診申報轉換量
        { name: 'CASEFROM', type: 'string' }, //標案來源
        { name: 'CONTRACNO', type: 'string' }, //合約碼
        { name: 'CONTRACTPRICE', type: 'string' }, //合約單價
        { name: 'COSTAMOUNT', type: 'string' }, //進價
        { name: 'DENTALREFFLAG', type: 'string' }, //牙科轉診加成
        { name: 'DRUGCASEFROM', type: 'string' }, //藥品標案來源
        { name: 'ENDDATETIME', type: 'string' }, //結束日期時間
        { name: 'EXAMINEDISCFLAG', type: 'string' }, //檢查折扣否
        { name: 'EXECFLAG', type: 'string' }, //執行否
        { name: 'HOSPEMGFLAG', type: 'string' }, //自費急件加成
        { name: 'HOSPKIDFLAG', type: 'string' }, //自費兒童加成
        { name: 'INSUAMOUNT1', type: 'string' }, //健保價
        { name: 'INSUAMOUNT2', type: 'string' }, //健保點數二
        { name: 'INSUEMGFLAG', type: 'string' }, //健保急件加成
        { name: 'INSUKIDFLAG', type: 'string' }, //兒童加成否
        { name: 'INSUORDERCODE', type: 'string' }, //健保碼
        { name: 'INSUSIGNI', type: 'string' }, //健保負擔碼(住院)
        { name: 'INSUSIGNO', type: 'string' }, //健保負擔碼(門診)
        { name: 'MAMAGEFLAG', type: 'string' }, //是否優惠
        { name: 'MAMAGERATE', type: 'string' }, //優惠%
        { name: 'ORIGINALPRODUCER', type: 'string' }, //製造廠名稱
        { name: 'PAYAMOUNT1', type: 'string' }, //自費價
        { name: 'PAYAMOUNT2', type: 'string' }, //自付金額2
        { name: 'PPFPERCENTAGE', type: 'string' }, //提成百分比
        { name: 'PPFTYPE', type: 'string' }, //提成類別碼
        { name: 'PTRESOLUTIONCLASS', type: 'string' }, //藥審會決議合約碼
        { name: 'STOCKTRANSQTYI', type: 'string' }, //住院扣庫轉換量
        { name: 'STOCKTRANSQTYO', type: 'string' }, //門診扣庫轉換量
        { name: 'SUPPLYNO', type: 'string' }, //廠商代碼
        { name: 'AGGREGATECODE', type: 'string' }, //累計用藥
        { name: 'AHFSCODE1', type: 'string' }, //藥品成份1
        { name: 'AHFSCODE2', type: 'string' }, //藥品成份2
        { name: 'AHFSCODE3', type: 'string' }, //藥品成份3
        { name: 'AHFSCODE4', type: 'string' }, //藥品成份4
        { name: 'AIRDELIVERY', type: 'string' }, //可氣送
        { name: 'ANTIBIOTICSCODE', type: 'string' }, //抗生素等級
        { name: 'APPENDMATERIALFLAG', type: 'string' }, //健保帶材料否
        { name: 'ATCCODE1', type: 'string' }, //藥理分類 ATCCODE1
        { name: 'ATCCODE2', type: 'string' }, //藥理分類 ATCCODE2
        { name: 'ATCCODE3', type: 'string' }, //藥理分類 ATCCODE3
        { name: 'ATCCODE4', type: 'string' }, //藥理分類 ATCCODE4
        { name: 'ATTACHUNIT', type: 'string' }, //申報(計價)單位
        { name: 'BATCHNOFLAG', type: 'string' }, //需有批號效期品項
        { name: 'BIOLOGICALAGENT', type: 'string' }, //生物製劑
        { name: 'BLOODPRODUCT', type: 'string' }, //血液製劑
        { name: 'BUYORDERFLAG', type: 'string' }, //買斷藥
        { name: 'CARRYKINDI', type: 'string' }, //住院消耗歸整
        { name: 'CARRYKINDO', type: 'string' }, //門急消耗歸整
        { name: 'CHANGEABLEFLAG', type: 'string' }, //可線上異動否
        { name: 'CHECKINSWITCH', type: 'string' }, //是否需報到
        { name: 'COLDSTORAGEFLAG', type: 'string' }, //冷藏存放
        { name: 'COSTEXCLUDECLASS', type: 'string' }, //成本排除特殊類別
        { name: 'CURETYPE', type: 'string' }, //特殊治療種類
        { name: 'DANGERDRUGFLAG', type: 'string' }, //高警訊藥品
        { name: 'DANGERDRUGMEMO', type: 'string' }, //高警訊藥品提示
        { name: 'DCL', type: 'string' }, //事前專審藥品替換群組
        { name: 'DOHLICENSENO', type: 'string' }, //衛生署核准字號
        { name: 'DOSE', type: 'string' }, //預設劑量
        { name: 'DRUGELEMCODE1', type: 'string' }, //藥品成份1
        { name: 'DRUGELEMCODE2', type: 'string' }, //藥品成份2
        { name: 'DRUGELEMCODE3', type: 'string' }, //藥品成份3
        { name: 'DRUGELEMCODE4', type: 'string' }, //藥品成份4
        { name: 'DRUGHOSPBEGINDATE', type: 'string' }, //藥品契約生效起日
        { name: 'DRUGHOSPENDDATE', type: 'string' }, //藥品契約生效迄日
        { name: 'DRUGPARENTCODE1', type: 'string' }, //成份母層代碼1
        { name: 'DRUGPARENTCODE2', type: 'string' }, //成份母層代碼2
        { name: 'DRUGPARENTCODE3', type: 'string' }, //成份母層代碼3
        { name: 'DRUGPARENTCODE4', type: 'string' }, //成份母層代碼4
        { name: 'EXAMINEDRUGFLAG', type: 'string' }, //檢驗用藥
        { name: 'EXCEPTIONCODE', type: 'string' }, //例外備註碼
        { name: 'EXCEPTIONFLAG', type: 'string' }, //是否有例外備註
        { name: 'EXCLUDEFLAG', type: 'string' }, //除外項目
        { name: 'EXORDERFLAG', type: 'string' }, //特殊品項
        { name: 'FIXDOSEFLAG', type: 'string' }, //預設劑量
        { name: 'FIXPATHNOFLAG', type: 'string' }, //限制途徑
        { name: 'FREEZING', type: 'string' }, //是否需冷凍
        { name: 'FREQNOI', type: 'string' }, //住院給藥頻率
        { name: 'FREQNOO', type: 'string' }, //門診給藥頻率
        { name: 'GERIATRIC', type: 'string' }, //老年人劑量調整
        { name: 'HEPATITISCODE', type: 'string' }, //BC肝用藥註記
        { name: 'HIGHPRICEFLAG', type: 'string' }, //高價用藥
        { name: 'HOSPCHARGEID1', type: 'string' }, //院內費用類別1(院內歸屬)
        { name: 'HOSPCHARGEID2', type: 'string' }, //院內費用類別2(院內歸屬)
        { name: 'HOSPEXAMINEFLAG', type: 'string' }, //內審用藥
        { name: 'HOSPEXAMINEQTYFLAG', type: 'string' }, //內審限制用量
        { name: 'INPDISPLAYFLAG', type: 'string' }, //住院醫囑顯示
        { name: 'INSUCHARGEID1', type: 'string' }, //健保費用類別1(健保歸屬)
        { name: 'INSUCHARGEID2', type: 'string' }, //健保費用類別2(健保歸屬)
        { name: 'INSUOFFERFLAG', type: 'string' }, //保險給付否(Y/N)
        { name: 'ISCURECODE', type: 'string' }, //同療程項目
        { name: 'LIGHTAVOIDFLAG', type: 'string' }, //避光存放
        { name: 'LIMITEDQTYI', type: 'string' }, //住院倍數核發
        { name: 'LIMITEDQTYO', type: 'string' }, //門診倍數核發
        { name: 'LIMITFLAG', type: 'string' }, //開立限制
        { name: 'LIVERLIMITED', type: 'string' }, //肝功能不良需調整劑量
        { name: 'MACHINEFLAG', type: 'string' }, //藥包機品項
        { name: 'MAINCUREITEM', type: 'string' }, //特定治療項目
        { name: 'MAXDAYSI', type: 'string' }, //住院限制開立日數
        { name: 'MAXDAYSO', type: 'string' }, //門診限制開立日數
        { name: 'MAXQTYI', type: 'string' }, //住院限制開立數量
        { name: 'MAXQTYO', type: 'string' }, //門診限制開立數量
        { name: 'MAXQTYPERDAY', type: 'string' }, //一日極量
        { name: 'MAXQTYPERTIME', type: 'string' }, //一次極量
        { name: 'MAXTAKETIMES', type: 'string' }, //限制次數
        { name: 'MILITARYEXCLUDECLASS', type: 'string' }, //軍醫局排除類別
        { name: 'NEEDOPDTYPEFLAG', type: 'string' }, //處置需報調劑方式
        { name: 'NEEDREGIONFLAG', type: 'string' }, //處置需報部位
        { name: 'ONLYROUNDFLAG', type: 'string' }, //不可剝半
        { name: 'OPENDATE', type: 'string' }, //系統啟用日期
        { name: 'OPERATIONFLAG', type: 'string' }, //手術碼
        { name: 'ORDERABLEDRUGFORM', type: 'string' }, //藥品劑型規格(電子簽章)
        { name: 'ORDERCHINNAME', type: 'string' }, //中文名稱
        { name: 'ORDERCHINUNIT', type: 'string' }, //中文單位
        { name: 'ORDERCODE', type: 'string' }, //院內代碼
        { name: 'ORDERCODESORT', type: 'string' }, //醫令排序
        { name: 'ORDERCONDCODE', type: 'string' }, //給付條文代碼
        { name: 'ORDERDAYS', type: 'string' }, //預設開立天數
        { name: 'ORDERDCFLAG', type: 'string' }, //停用碼
        { name: 'ORDEREASYNAME', type: 'string' }, //簡稱
        { name: 'ORDERENGNAME', type: 'string' }, //英文名稱
        { name: 'ORDERHOSPNAME', type: 'string' }, //院內名稱
        { name: 'ORDERKIND', type: 'string' }, //醫令類別(申報定義)
        { name: 'ORDERUNIT', type: 'string' }, //醫囑單位
        { name: 'ORDERUSETYPE', type: 'string' }, //醫令使用狀態
        { name: 'PATHNO', type: 'string' }, //院內給藥途徑(部位)代碼
        { name: 'PUBLICDRUGFLAG', type: 'string' }, //公藥否(Y/N)
        { name: 'RAREDISORDERFLAG', type: 'string' }, //罕見疾病用藥
        { name: 'RAYPOSITION', type: 'string' }, //放射部位
        { name: 'RENALLIMITED', type: 'string' }, //腎功能不良需調整劑量
        { name: 'REPORTFLAG', type: 'string' }, //是否發報告
        { name: 'RESEARCHDRUGFLAG', type: 'string' }, //研究用藥
        { name: 'RESTRICTCODE', type: 'string' }, //管制用藥
        { name: 'RESTRICTTYPE', type: 'string' }, //限制狀態
        { name: 'RETURNDRUGFLAG', type: 'string' }, //合理回流藥
        { name: 'RFIDCODE', type: 'string' }, //RFID條碼
        { name: 'SAFETYSYRINGE', type: 'string' }, //安全針具(衛材)
        { name: 'SCIENTIFICNAME', type: 'string' }, //成份名稱
        { name: 'SECTIONNO', type: 'string' }, //院內科別代碼
        { name: 'SENDUNITFLAG', type: 'string' }, //檢查類別否
        { name: 'SIGNFLAG', type: 'string' }, //是否需 Sign In
        { name: 'SINGLEITEMFLAG', type: 'string' }, //獨立處方箋
        { name: 'SOONCULLFLAG', type: 'string' }, //開立醫令即為報到
        { name: 'SPECIALORDERKIND', type: 'string' }, //專案碼
        { name: 'STOCKUNIT', type: 'string' }, //扣庫單位
        { name: 'SUBSTITUTE1', type: 'string' }, //替代院內代碼1
        { name: 'SUBSTITUTE2', type: 'string' }, //替代院內代碼2
        { name: 'SUBSTITUTE3', type: 'string' }, //替代院內代碼3
        { name: 'SUBSTITUTE4', type: 'string' }, //替代院內代碼4
        { name: 'SUBSTITUTE5', type: 'string' }, //替代院內代碼5
        { name: 'SUBSTITUTE6', type: 'string' }, //替代院內代碼6
        { name: 'SUBSTITUTE7', type: 'string' }, //替代院內代碼7
        { name: 'SUBSTITUTE8', type: 'string' }, //替代院內代碼8
        { name: 'SYMPTOMCHIN', type: 'string' }, //適應症(中文)
        { name: 'SYMPTOMENG', type: 'string' }, //適應症(英文)
        { name: 'TAKEKIND', type: 'string' }, //服用藥別
        { name: 'TDMFLAG', type: 'string' }, //TDM 藥品
        { name: 'TRANSCOMPUTEFLAG', type: 'string' }, //結轉計價
        { name: 'UDPOWDERFLAG', type: 'string' }, //UD磨粉
        { name: 'UDSERVICEFLAG', type: 'string' }, //UD服務(Y/N)
        { name: 'UNABLEPOWDERFLAG', type: 'string' }, //不可磨粉
        { name: 'VACCINE', type: 'string' }, //疫苗註記
        { name: 'VACCINECLASS', type: 'string' }, //疫苗類別
        { name: 'VALIDDAYSI', type: 'string' }, //住院效期日數
        { name: 'VALIDDAYSO', type: 'string' }, //門診效期日數
        { name: 'WEIGHTTYPE', type: 'string' }, //體重及安全量：計算別
        { name: 'WEIGHTUNITLIMIT', type: 'string' }, //體重及安全量：限制數量
        { name: 'ZEROCASESTATE', type: 'string' }, //零購品結案註記
        { name: 'APPLYTRANSQTY', type: 'string' }, //單位轉換量(院內/最小)
        { name: 'APPLYUNIT', type: 'string' }, //院內單位(預設扣庫單位)
        { name: 'ARMYORDERCODE', type: 'string' }, //軍品院內碼 民品所對應的軍品院內碼
        { name: 'CHINATTENTION', type: 'string' }, //注意事項(中文)
        { name: 'CHINSIDEEFFECT', type: 'string' }, //主要副作用(中文)
        { name: 'CLASSIFIEDARMYNO', type: 'string' }, //軍聯項次組別
        { name: 'COMMITTEECODE', type: 'string' }, //藥委會品項
        { name: 'COMMITTEEMEMO', type: 'string' }, //藥委會註記
        { name: 'COMPONENTNUNIT', type: 'string' }, //成份量及單位
        { name: 'COMPONENTNUNIT2', type: 'string' }, //成份量及單位2
        { name: 'COMPONENTNUNIT3', type: 'string' }, //成份量及單位3
        { name: 'COMPONENTNUNIT4', type: 'string' }, //成份量及單位4
        { name: 'CONTRACTEFFECTIVEDATE', type: 'string' }, //合約效期
        { name: 'DANGERBEGIN', type: 'string' }, //TDM 危急值 起
        { name: 'DANGEREND', type: 'string' }, //TDM 危急值 迄
        { name: 'DOHSYMPTOM', type: 'string' }, //衛生署核准適應症
        { name: 'DRUGAPPLYTYPE', type: 'string' }, //藥品請領類別
        { name: 'DRUGCLASS', type: 'string' }, //藥品類別
        { name: 'DRUGCLASSIFY', type: 'string' }, //藥品性質欄位
        { name: 'DRUGENGEXTERIOR', type: 'string' }, //藥品外觀(英文)
        { name: 'DRUGEXTERIOR', type: 'string' }, //藥品外觀
        { name: 'DRUGFORM', type: 'string' }, //藥品劑型
        { name: 'DRUGLEAFLETLINK', type: 'string' }, //藥品仿單檔名
        { name: 'DRUGMEMO', type: 'string' }, //處方集
        { name: 'DRUGPACKAGE', type: 'string' }, //藥品包裝
        { name: 'DRUGPICTURELINK', type: 'string' }, //藥品圖片檔名
        { name: 'DRUGTOTALAMOUNT', type: 'string' }, //體積量
        { name: 'DRUGTOTALAMOUNTUNIT', type: 'string' }, //體積單位
        { name: 'ENGATTENTION', type: 'string' }, //注意事項(英文)
        { name: 'ENGSIDEEFFECT', type: 'string' }, //主要副作用(英文)
        { name: 'FDASYMPTOM', type: 'string' }, //FDA核准適應症
        { name: 'GROUPARMYNO', type: 'string' }, //軍聯項次分類
        { name: 'INVENTORYFLAG', type: 'string' }, //盤點品項(Y/N)
        { name: 'ITEMARMYNO', type: 'string' }, //軍聯項次號
        { name: 'MANUFACTURER', type: 'string' }, //原製造商(廠牌)
        { name: 'MAXCURECONSISTENCY', type: 'string' }, //TDM 合理治療濃度上限
        { name: 'MINCURECONSISTENCY', type: 'string' }, //TDM 合理治療濃度下限
        { name: 'MULTIPRESCRIPTIONCODE', type: 'string' }, //藥品單複方
        { name: 'PARENTCODE', type: 'string' }, //母藥註記
        { name: 'PARENTORDERCODE', type: 'string' }, //母藥院內碼
        { name: 'MMCOPEARBEGINDE', type: 'string' }, //TDM 合理PEAK起
        { name: 'PEAREND', type: 'string' }, //TDM 合理PEAK迄
        { name: 'PREGNANTGRADE', type: 'string' }, //懷孕分級
        { name: 'PURCHASECASETYPE', type: 'string' }, //藥品採購案別
        { name: 'PURCHASETRANSQTY', type: 'string' }, //單位轉換量(採購/院內)
        { name: 'PURCHASEUNIT', type: 'string' }, //採購單位
        { name: 'SONTRANSQTY', type: 'string' }, //子藥轉換量
        { name: 'SPECNUNIT', type: 'string' }, //規格量及單位
        { name: 'STOCKSOURCECODE', type: 'string' }, //來源代碼
        { name: 'STOCKUSECODE', type: 'string' }, //扣庫規則分類
        { name: 'SUCKLESECURITY', type: 'string' }, //授乳安全性
        { name: 'TDMMEMO1', type: 'string' }, //TDM 備註1
        { name: 'TDMMEMO2', type: 'string' }, //TDM 備註2
        { name: 'TDMMEMO3', type: 'string' }, //TDM 備註3
        { name: 'TROUGHBEGIN', type: 'string' }, //TDM 合理 Trough 起
        { name: 'TROUGHEND', type: 'string' }, //TDM 合理 Trough 迄
        { name: 'WARN', type: 'string' }, //警語
        { name: 'YEARARMYNO', type: 'string' }, //軍聯項次年號
    ]
});