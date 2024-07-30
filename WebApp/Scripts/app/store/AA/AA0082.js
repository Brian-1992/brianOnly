Ext.define('WEBAPP.store.AA.AA0082', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA.AA0082',
    pageSize: 1000, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'ROWITEM', direction: 'ASC' }], // 預設排序欄位 
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0082/All', // store 來源
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});