Ext.define('WEBAPP.model.AA.AA0056M', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MMCODE', type: 'string' }, //母藥院內碼(畫面)
        { name: 'MMNAME_E', type: 'string' }, //藥品名稱(畫面)
        { name: 'E_SONTRANSQTY', type: 'string' }, //母藥轉換量(畫面)
        { name: 'E_PARCODE', type: 'string' }, //母藥註記代碼
        { name: 'E_PARCODE_NAME', type: 'string' }, //母藥註記名稱(畫面)
        { name: 'E_PARCODE_CODE', type: 'string' }, //母藥註記代碼+名稱
        { name: 'UPDATE_IP', type: 'string' }, //異動IP
        { name: 'UPDATE_TIME', type: 'string' }, //異動日期
        { name: 'UPDATE_USER', type: 'string' } //異動人員
    ]
});