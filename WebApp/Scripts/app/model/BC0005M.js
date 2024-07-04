Ext.define('WEBAPP.model.BC0005M', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'PO_NO', type: 'string' }, //訂單編號(畫面)
        { name: 'AGEN_NO', type: 'string' }, //廠商代碼
        { name: 'AGEN_NAMEC', type: 'string' }, //廠商名稱(畫面)
        { name: 'PO_TIME', type: 'string' }, //訂單時間
        { name: 'M_CONTID', type: 'string' }, //合約識別碼
        { name: 'PO_STATUS', type: 'string' }, //訂單號碼(畫面)
        { name: 'PO_STATUS_CODE', type: 'string' }, //訂單狀態代碼+名稱(瀏覽)
        { name: 'PO_STATUS_NAME', type: 'string' }, //訂單狀態名稱(畫面)
        { name: 'CREATE_TIME', type: 'string' }, //建立日期
        { name: 'CREATE_USER', type: 'string' }, //建立人員
        { name: 'UPDATE_TIME', type: 'string' }, //異動日期
        { name: 'UPDATE_USER', type: 'string' }, //異動人員
        { name: 'UPDATE_IP', type: 'string' }, //異動IP
        { name: 'MEMO', type: 'string' }, //主備註-MAIL內容(畫面)
        { name: 'ISCONFIRM', type: 'string' }, //是否確認彙總
        { name: 'ISBACK', type: 'string' }, //是否回覆
        { name: 'PHONE', type: 'string' }, //廠商電話
        { name: 'SMEMO', type: 'string' }, //特殊備註-MAIL內容特別註記(紅色顯示)(畫面)
        { name: 'ISCOPY', type: 'string' }, //Y-已複製到外網,N-未複製
        { name: 'SDN', type: 'string' }, //來源單號(畫面) from PH_SMALL_M(DN)
    ]
});