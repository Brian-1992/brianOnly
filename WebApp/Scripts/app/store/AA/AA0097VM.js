Ext.define('WEBAPP.store.AA.AA0097VM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA0097M',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        timeout: 90000,
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0097/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});