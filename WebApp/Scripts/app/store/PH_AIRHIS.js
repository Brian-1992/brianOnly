Ext.define('WEBAPP.store.PH_AIRHIS', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.PH_AIRHIS',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'AGEN_NO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BH0005/His',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});