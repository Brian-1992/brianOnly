Ext.define('WEBAPP.store.MeDocd', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.MeDocd',
    pageSize: 1000, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'SEQ', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0015/AllMeDocd',
        reader: {
            type: 'json',
            rootProperty: 'etts'
            //totalProperty: 'rc'
        }
    }
});