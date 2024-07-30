Ext.define('WEBAPP.store.BD0008', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.BD0008',
    pageSize: 99999999, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'RECYM', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BD0008/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});