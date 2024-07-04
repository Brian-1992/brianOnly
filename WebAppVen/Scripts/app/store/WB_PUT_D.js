Ext.define('WEBAPP.store.WB_PUT_D', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.WB_PUT_D',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'TXTDAY', direction: 'DESC' }, { property: 'SEQ', direction: 'DESC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BH0003/AllD',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});