Ext.define('WEBAPP.store.AA.AA0184', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA.AA0184',
    pageSize: 15, // 每頁顯示筆數
    remoteSort: true,
    sorters: [
        { property: 'DOCNO', direction: 'ASC' },
        { property: 'MAT_CLASS', direction: 'ASC' },
        { property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0184/All',
        timeout: 9000000,
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});