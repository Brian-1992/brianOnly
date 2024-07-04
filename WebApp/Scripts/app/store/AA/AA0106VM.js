Ext.define('WEBAPP.store.AA.AA0106VM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA0106M',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'WH_NO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        timeout: 90000,
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0106/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});