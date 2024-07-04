Ext.define('WEBAPP.store.AA.AA0059VM', {
    extend: 'Ext.app.ViewModel',
    alias: 'viewmodel.AA0059M',
    requires: [
        'WEBAPP.model.MI_WHMAST',
        'WEBAPP.model.MI_MAST',
        'WEBAPP.model.AA0059M',
        'WEBAPP.model.ParamD'
    ],
    stores: {
        MI_WHMAST: {
            model: 'WEBAPP.model.MI_WHMAST',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'WH_NO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AA0059/AllMI_WHMAST',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        MI_MAST: {
            model: 'WEBAPP.model.MI_MAST',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AA0059/AllMI_MAST',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        MI_WHMM: {
            model: 'WEBAPP.model.AA0059M',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'WH_NO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AA0059/AllMI_WHMM',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        MI_MMWH: {
            model: 'WEBAPP.model.AA0059M',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AA0059/AllMI_MMWH',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }
});