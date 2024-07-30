Ext.define('WEBAPP.store.MI_WHMAST', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.MI_WHMAST',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'WH_NO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0039/All',
        reader: {
            type: 'json',
            rootProperty: 'etts'
        }
    }
});