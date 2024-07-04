Ext.define('WEBAPP.store.CE0019VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.CHK_CE0019VM'
    ],
    stores: {
        allini: {
            model: 'WEBAPP.model.CHK_CE0019VM',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                timeout: 120000,
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0019/AllINI',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        
    }
});