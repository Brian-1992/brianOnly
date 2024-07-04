Ext.define('WEBAPP.store.AA.AA0056M', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA.AA0056M',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0056/AllM',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});