Ext.define('WEBAPP.store.AA.AA0177', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA.AA0177',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0177/All',
        timeout: 9000000,
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});