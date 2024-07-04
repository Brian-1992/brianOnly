Ext.define('WEBAPP.store.AB0053_1', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AB0053_1',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MMCODE', direction: 'ASC' }, { property: 'WH_NO', direction: 'ASC' }, { property: 'LOT_NO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0053/All_1',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});