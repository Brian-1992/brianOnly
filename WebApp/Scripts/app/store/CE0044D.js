Ext.define('WEBAPP.store.CE0044D', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.CE0044D',
    pageSize: 10, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'F1', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CE0044/AllD',
        timeout: 9000000,
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});