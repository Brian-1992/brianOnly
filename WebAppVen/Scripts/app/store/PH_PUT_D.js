Ext.define('WEBAPP.store.PH_PUT_D', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.PH_PUT_D',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'TXTDAY', direction: 'DESC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BB0002/AllD',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});