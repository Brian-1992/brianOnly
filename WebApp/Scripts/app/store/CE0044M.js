Ext.define('WEBAPP.store.CE0044M', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.CE0044M',
    pageSize: 99999, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'F1', direction: 'ASC' },{ property: 'F2', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CE0044/AllM',
        timeout: 0,
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});