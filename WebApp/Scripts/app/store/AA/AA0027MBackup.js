Ext.define('WEBAPP.store.AA.AA0027MBackup', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA.AA0027MBackup',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'APPTIME', direction: 'DESC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0027Backup/AllM',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});