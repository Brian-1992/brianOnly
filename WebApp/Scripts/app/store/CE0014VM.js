Ext.define('WEBAPP.store.CE0014VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.BC_WHCHKID',
        'WEBAPP.model.CE0014'
    ],
    stores: {
        BC_WHCHKID: {
            model: 'WEBAPP.model.BC_WHCHKID',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'WH_CHKUID', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0014/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        CE0014_URID: {
            model: 'WEBAPP.model.CE0014',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'TUSER', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0014/Urid',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
    }
});