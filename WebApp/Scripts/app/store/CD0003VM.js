Ext.define('WEBAPP.store.CD0003VM', {
    extend: 'Ext.app.ViewModel',
    alias: 'viewmodel.CD0003M',
    requires: [
        'WEBAPP.model.MI_WHMAST',
        'WEBAPP.model.UR_ID',
        'WEBAPP.model.CD0003M',
        'WEBAPP.model.ParamD'
    ],
    stores: {
        UR_ID: {
            model: 'WEBAPP.model.UR_ID',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'TUSER', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/CD0003/ID',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        BC_WHID: {
            model: 'WEBAPP.model.CD0003M',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'WH_NO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CD0003/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
    }
});