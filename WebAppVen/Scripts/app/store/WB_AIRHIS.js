Ext.define('WEBAPP.store.WB_AIRST', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.WB_AIRST',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'AGEN_NO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BH0004/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'

        }
    }
});