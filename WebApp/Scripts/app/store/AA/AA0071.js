Ext.define('WEBAPP.store.AA.AA0071', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA.AA0071',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'APPTIME', direction: 'ASC' }], // 預設排序欄位 
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0071/All', // store 來源
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});