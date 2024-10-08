﻿Ext.define('WEBAPP.model.MiMast', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MMCODE', type: 'string' },
        { name: 'MMNAME', type: 'string' },     // 自訂 MMCODE + MMNAME_C
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'MAT_CLASS', type: 'string' },
        { name: 'MAT_CLASS_SUB', type: 'string' },  // 物料分類子類別
        { name: 'BASE_UNIT', type: 'string' },
        { name: 'AUTO_APLID', type: 'string' },
        { name: 'M_STOREID', type: 'string' },
        { name: 'M_CONTID', type: 'string' },
        { name: 'M_IDKEY', type: 'string' },
        { name: 'M_INVKEY', type: 'string' },
        { name: 'M_NHIKEY', type: 'string' },
        { name: 'M_GOVKEY', type: 'string' },
        { name: 'M_VOLL', type: 'string' },
        { name: 'M_VOLW', type: 'string' },
        { name: 'M_VOLH', type: 'string' },
        { name: 'M_VOLC', type: 'string' },
        { name: 'M_SWAP', type: 'string' },
        { name: 'M_MATID', type: 'string' },
        { name: 'M_SUPPLYID', type: 'string' },
        { name: 'M_CONSUMID', type: 'string' },
        { name: 'M_PAYKIND', type: 'string' },
        { name: 'M_PAYID', type: 'string' },
        { name: 'M_TRNID', type: 'string' },
        { name: 'M_APPLYID', type: 'string' },
        { name: 'M_PHCTNCO', type: 'string' },
        { name: 'M_ENVDT', type: 'string' },
        { name: 'M_DISTUN', type: 'string' },
        { name: 'M_AGENNO', type: 'string' },
        { name: 'M_AGENLAB', type: 'string' },
        { name: 'M_PURUN', type: 'string' },
        { name: 'M_CONTPRICE', type: 'string' },
        { name: 'M_DISCPERC', type: 'string' },
        { name: 'E_SUPSTATUS', type: 'string' },
        { name: 'E_MANUFACT', type: 'string' },
        { name: 'E_IFPUBLIC', type: 'string' },
        { name: 'E_STOCKTYPE', type: 'string' },
        { name: 'E_SPECNUNIT', type: 'string' },
        { name: 'E_COMPUNIT', type: 'string' },
        { name: 'E_YRARMYNO', type: 'string' },
        { name: 'E_ITEMARMYNO', type: 'string' },
        { name: 'E_GPARMYNO', type: 'string' },
        { name: 'E_CLFARMYNO', type: 'string' },
        { name: 'E_CODATE', type: 'date' },
        { name: 'E_CODATE_T', type: 'string' },
        { name: 'E_PRESCRIPTYPE', type: 'string' },
        { name: 'E_DRUGCLASS', type: 'string' },
        { name: 'E_DRUGCLASSIFY', type: 'string' },
        { name: 'E_DRUGFORM', type: 'string' },
        { name: 'E_COMITMEMO', type: 'string' },
        { name: 'E_COMITCODE', type: 'string' },
        { name: 'E_INVFLAG', type: 'string' },
        { name: 'E_PURTYPE', type: 'string' },
        { name: 'E_SOURCECODE', type: 'string' },
        { name: 'E_DRUGAPLTYPE', type: 'string' },
        { name: 'E_ARMYORDCODE', type: 'string' },
        { name: 'E_PARCODE', type: 'string' },
        { name: 'E_PARORDCODE', type: 'string' },
        { name: 'E_SONTRANSQTY', type: 'string' },
        { name: 'CANCEL_ID', type: 'string' },
        { name: 'E_RESTRICTCODE', type: 'string' },
        { name: 'E_ORDERDCFLAG', type: 'string' },
        { name: 'E_HIGHPRICEFLAG', type: 'string' },
        { name: 'E_RETURNDRUGFLAG', type: 'string' },
        { name: 'E_RESEARCHDRUGFLAG', type: 'string' },
        { name: 'E_VACCINE', type: 'string' },
        { name: 'E_TAKEKIND', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'WEXP_ID', type: 'string' },
        { name: 'WLOC_ID', type: 'string' },
        { name: 'E_PATHNO', type: 'string' },
        { name: 'E_ORDERUNIT', type: 'string' },
        { name: 'E_FREQNOO', type: 'string' },
        { name: 'E_FREQNOI', type: 'string' },
        { name: 'CONTRACNO', type: 'string' },
        { name: 'UPRICE', type: 'string' },
        { name: 'DISC_CPRICE', type: 'string' },
        { name: 'DISC_UPRICE', type: 'string' },
        { name: 'EASYNAME', type: 'string' },
        { name: 'CANCEL_NOTE', type: 'string' },
        { name: 'NHI_PRICE', type: 'string' },
        { name: 'HOSP_PRICE', type: 'string' },
        { name: 'PFILE_ID', type: 'string' },
        { name: 'AGEN_NAMEC', type: 'string' },
        { name: 'MAT_CLSID', type: 'string' },
        { name: 'BEGINDATE', type: 'string' },
        { name: 'ENDDATE', type: 'string' },
        { name: 'E_STOCKTRANSQTYI', type: 'string' },
        { name: 'E_SCIENTIFICNAME', type: 'string' },
        { name: 'BEGINDATE_DATE', type: 'string' },
        { name: 'ENDDATE_DATE', type: 'string' },
        { name: 'ISWILLING', type: 'string' },
        { name: 'DISCOUNT_QTY', type: 'string' },
        { name: 'DISC_COST_UPRICE', type: 'string' },
        { name: 'DRUGSNAME', type: 'string' },
        { name: 'DRUGHIDE', type: 'string' },
        { name: 'CANCEL_ID_DESC', type: 'string' },
        { name: 'E_ORDERDCFLAG_DESC', type: 'string' },
        { name: 'HEALTHOWNEXP', type: 'string' },
        { name: 'ISSUESUPPLY', type: 'string' },
        { name: 'BASE_UNIT_DESC', type: 'string' },
        { name: 'M_PURUN_DESC', type: 'string' },
        { name: 'TRUTRATE', type: 'string' },
        { name: 'MAT_CLASS_SUB_DESC', type: 'string' },
        { name: 'E_RESTRICTCODE_DESC', type: 'string' },
        { name: 'WARBAK', type: 'string' },
        { name: 'WARBAK_DESC', type: 'string' },
        { name: 'ONECOST', type: 'string' },
        { name: 'ONECOST_DESC', type: 'string' },
        { name: 'HEALTHPAY', type: 'string' },
        { name: 'HEALTHPAY_DESC', type: 'string' },
        { name: 'COSTKIND', type: 'string' },
        { name: 'COSTKIND_DESC', type: 'string' },
        { name: 'WASTKIND', type: 'string' },
        { name: 'WASTKIND_DESC', type: 'string' },
        { name: 'SPXFEE', type: 'string' },
        { name: 'SPXFEE_DESC', type: 'string' },
        { name: 'ORDERKIND', type: 'string' },
        { name: 'ORDERKIND_DESC', type: 'string' },
        { name: 'DRUGKIND', type: 'string' },
        { name: 'DRUGKIND_DESC', type: 'string' },
        { name: 'SPDRUG', type: 'string' },
        { name: 'SPDRUG_DESC', type: 'string' },
        { name: 'FASTDRUG', type: 'string' },
        { name: 'FASTDRUG_DESC', type: 'string' },
        { name: 'MIMASTHIS_SEQ', type: 'string' },
        { name: 'CASENO', type: 'string' },
        { name: 'M_CONTID_DESC', type: 'string' },
        { name: 'CONTRACTAMT', type: 'string' },
        { name: 'CONTRACTSUM', type: 'string' },
        { name: 'TOUCHCASE', type: 'string' },
        { name: 'TOUCHCASE_DESC', type: 'string' },
        { name: 'BEGINDATE_14', type: 'string' },
        { name: 'BEGINDATE_14_T', type: 'string' },
        { name: 'EFFSTARTDATE', type: 'string' },
        { name: 'EFFSTARTDATE_T', type: 'string' },
        { name: 'EFFENDDATE', type: 'string' },
        { name: 'EFFENDDATE_T', type: 'string' },
        { name: 'ISSPRICEDATE', type: 'string' },
        { name: 'ISSPRICEDATE_T', type: 'string' },
        { name: 'CASEDOCT', type: 'string' },
        { name: 'BARCODE', type: 'string' },
        { name: 'MMCODE_BARCODE', type: 'string' },
        { name: 'COMMON', type: 'string' },
        { name: 'SPMMCODE', type: 'string' },
        { name: 'UNITRATE', type: 'string' },
        { name: 'APPQTY_TIMES', type: 'string' },
        { name: 'ISIV', type: 'string' },
        { name: 'JBID_RCRATE', type: 'string' },
    ]
});