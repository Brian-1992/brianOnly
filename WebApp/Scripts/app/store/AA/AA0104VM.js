Ext.define('WEBAPP.store.AA.AA0104VM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA0104M',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'WH_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        timeout: 180000,
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0104/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});