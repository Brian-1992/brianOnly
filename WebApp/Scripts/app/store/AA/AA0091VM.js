Ext.define('WEBAPP.store.AA.AA0091VM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA0091M',
    pageSize: 50, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'ORDERCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0091/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});