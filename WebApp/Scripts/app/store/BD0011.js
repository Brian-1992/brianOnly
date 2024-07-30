Ext.define('WEBAPP.store.BD0011', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.BD0011',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'SEND_DT', direction: 'ASC' }, { property: 'DOCID', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BD0011/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});