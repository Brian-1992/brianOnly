Ext.define('WEBAPP.store.DGMISS_SUPPLY', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.DGMISS_SUPPLY',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'SUPPLY_INID', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0226/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty:'rc'
        }
    }
});