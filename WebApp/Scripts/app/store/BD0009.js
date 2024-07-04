Ext.define('WEBAPP.store.BD0009', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.BD0009',
    pageSize: 20000, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'AGEN_NAME', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        timeout: 600000,
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BD0009/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});