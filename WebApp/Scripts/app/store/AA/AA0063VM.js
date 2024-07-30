Ext.define('WEBAPP.store.AA.AA0063VM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA0063M',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'APPTIME', direction: 'ASC' }, { property: 'DOCNO', direction: 'ASC' }, { property: 'SEQ', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0063/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});