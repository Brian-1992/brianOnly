Ext.define('WEBAPP.model.BG0003', {
    extend: 'Ext.data.Model',
    fields: [
        ///////////////超過十萬品項///////////////
        { name: 'YYYMM', type: 'string' }, //1_年月
        { name: 'MMCODE', type: 'string' }, //1_院內碼
        { name: 'MMNAME_E', type: 'string' }, //1_英文品名
        { name: 'MMNAME_C', type: 'string' }, //1_中文品名
        { name: 'BASE_UNIT', type: 'string' }, //1_計量單位
        { name: 'UPRICE', type: 'string' }, //1_單價
        { name: 'M_AGENNO', type: 'string' }, //1_廠商碼
        { name: 'AGEN_NAMEC', type: 'string' }, //1_廠商名稱
        { name: 'MAT_CLASS', type: 'string' }, //1_物料類別
        { name: 'APPQTY', type: 'string' }, //1_數量
        { name: 'ESTPAY', type: 'string' }, //1_預估申購金額

        ///////////////超過十萬單位///////////////
        { name: 'YYYMM_2', type: 'string' }, //2_年月
        { name: 'INID', type: 'string' }, //2_責任中心
        { name: 'DOCNO', type: 'string' }, //2_申請單號
        { name: 'MMCODE_2', type: 'string' }, //2_院內碼
        { name: 'MMNAME_E_2', type: 'string' }, //2_英文品名
        { name: 'MMNAME_C_2', type: 'string' }, //2_中文品名
        { name: 'BASE_UNIT_2', type: 'string' }, //2_計量單位
        { name: 'UPRICE_2', type: 'string' }, //2_單價
        { name: 'APPQTY_2', type: 'string' }, //2_數量
        { name: 'ESTPAY_2', type: 'string' }, //2_預估申購金額

        ///////////////尚未開單單位///////////////
        { name: 'APPDEPT', type: 'string' }, //3_責任中心
        { name: 'INID_NAME', type: 'string' } //3_責任中心名稱
    ]
});