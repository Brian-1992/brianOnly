Ext.define('WEBAPP.store.CE0027VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.CE0027'
    ],
    stores: {
        MasterAll: {
            model: 'WEBAPP.model.CE0027',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0027/MasterAll',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }
});