Ext.define('WEBAPP.model.CD0005D', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'SEQ', type: 'string' }, //項次(畫面)
        { name: 'MMCODE', type: 'string' }, //院內碼(畫面)
        { name: 'MMNAME_C', type: 'string' }, //中文品名(畫面)
        { name: 'MMNAME_E', type: 'string' }, //英文品名(畫面)
        { name: 'APPQTY', type: 'string' }, //申請數量(畫面)
        { name: 'ACT_PICK_QTY', type: 'string' }, //揀貨數量(畫面)
        { name: 'BASE_UNIT', type: 'string' } //撥補單位(畫面)
    ]
});