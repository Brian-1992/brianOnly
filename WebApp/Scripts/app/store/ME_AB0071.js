Ext.define('WEBAPP.store.ME_AB0071', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.ME_AB0071',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0071/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});