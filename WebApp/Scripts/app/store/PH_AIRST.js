Ext.define('WEBAPP.store.PH_AIRST', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.PH_AIRST',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'AGEN_NO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BH0005/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'

        }
    }
});