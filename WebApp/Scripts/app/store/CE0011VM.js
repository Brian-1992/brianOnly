Ext.define('WEBAPP.store.CE0011VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.CHK_MAST',
        'WEBAPP.model.CHK_DETAIL',
        'WEBAPP.model.BC_WHCHKID'
    ],
    stores: {
        MasterAll: {
            model: 'WEBAPP.model.CHK_MAST',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'CHK_NO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0011/MasterAll',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        DetailInclude: {
            model: 'WEBAPP.model.CHK_DETAIL',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0011/GetDetails',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }
});