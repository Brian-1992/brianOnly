Ext.define('WEBAPP.store.AA.AA0027D', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA.AA0027D',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MMCODE', direction: 'ASC' },{ property: 'SEQ', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0027/AllD',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});