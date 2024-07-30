Ext.define('WEBAPP.store.CE0040D', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.CE0040D',
    pageSize: 10, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CE0040/AllD',
        timeout: 9000000,
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});