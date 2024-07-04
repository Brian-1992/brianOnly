Ext.define('WEBAPP.store.BG0008', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.BG0008',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'AGEN_NO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BG0008/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});