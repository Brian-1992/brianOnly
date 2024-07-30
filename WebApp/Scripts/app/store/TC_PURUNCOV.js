Ext.define('WEBAPP.store.TC_PURUNCOV', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.TC_PURUNCOV',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'PUR_UNIT', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/GA0004/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});