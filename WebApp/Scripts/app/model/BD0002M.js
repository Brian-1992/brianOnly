Ext.define('WEBAPP.model.BD0002M', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'PR_NO', type: 'string' }, //申購單號
        { name: 'PR_TIME', type: 'string' }, //申購時間
        { name: 'MAT_CLASS', type: 'string' }, //物料類別
        { name: 'M_STOREID', type: 'string' }, //庫備識別碼
        { name: 'M_STOREID_NAME', type: 'string' }, //庫備識別碼名稱
        { name: 'M_STOREID_CODE', type: 'string' }, //庫備識別碼+名稱
        { name: 'PR_STATUS', type: 'string' }, //申購單狀態代碼
        { name: 'PR_STATUS_NAME', type: 'string' }, //申購單狀態名稱
        { name: 'PR_STATUS_CODE', type: 'string' }, //申購單狀態代碼+名稱
        { name: 'XACTION', type: 'string' }, //申購類別代碼
        { name: 'XACTION_NAME', type: 'string' }, //申購類別名稱
        { name: 'XACTION_CODE', type: 'string' } //申購類別代碼+名稱
    ]
});