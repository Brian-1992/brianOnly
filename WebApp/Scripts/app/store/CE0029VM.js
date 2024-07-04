Ext.define('WEBAPP.store.CE0029VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.CHK_WHID',
        'WEBAPP.model.CE0014'
    ],
    stores: {
        All: {
            model: 'WEBAPP.model.CHK_WHID',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'WH_USERID', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0029/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        Urid: {
            model: 'WEBAPP.model.CE0014',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'UNA', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0029/Urid',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
    }
});