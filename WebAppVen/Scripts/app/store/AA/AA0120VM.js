Ext.define('WEBAPP.store.AA.AA0120VM', {
    extend: 'Ext.app.ViewModel',
    //alias: 'viewmodel.AA0015',
    requires: [
        'WEBAPP.model.MI_UNITEXCH',
        'WEBAPP.model.MI_MAST',
        'WEBAPP.model.MI_UNITCODE',
        'WEBAPP.model.PH_VENDER'
    ],
    stores: {
        MI_UNITEXCH: {
            model: 'WEBAPP.model.MI_UNITEXCH',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AA0120/All',
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
                    read: 'POST' // by default GET
                },
                url: '/api/AA0120/GetMiMast',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                }
            }
        },
        MI_UNITCODE: {
            model: 'WEBAPP.model.MI_UNITCODE',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'UNIT_CODE', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AA0120/GetMiUnitcode',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                }
            }
        },
        PH_VENDOR: {
            model: 'WEBAPP.model.PH_VENDER',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'AGEN_NO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AA0120/GetPhVendor',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                }
            }
        }
    }
});