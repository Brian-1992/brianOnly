Ext.define('WEBAPP.store.AA.AA0139', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA.AA0139',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'BID_NO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0139/All_AA0139',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});