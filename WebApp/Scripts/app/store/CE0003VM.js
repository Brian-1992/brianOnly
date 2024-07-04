Ext.define('WEBAPP.store.CE0003VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.CHK_MAST',
        'WEBAPP.model.CHK_DETAIL',
        'WEBAPP.model.BC_WHCHKID'
    ],
    stores: {
        DetailsMulti: {
            model: 'WEBAPP.model.CHK_MAST',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0016/DetailsMulti',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        
    }
});