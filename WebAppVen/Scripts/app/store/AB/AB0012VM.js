Ext.define('WEBAPP.store.AB.AB0012VM', {
    extend: 'Ext.app.ViewModel',
    //alias: 'viewmodel.AA0015',
    requires: [
        'WEBAPP.model.ME_DOCM',
        'WEBAPP.model.ME_DOCD',
        'WEBAPP.model.MI_WHMAST',
        'WEBAPP.model.UserInfo',
        'WEBAPP.model.ParamD',
        'WEBAPP.model.MI_MAST'
    ],
    stores: {
        ME_DOCM: {
            model: 'WEBAPP.model.ME_DOCM',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'DOCNO', direction: 'DESC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AB0012/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        ME_DOCD: {
            model: 'WEBAPP.model.ME_DOCD',
            pageSize: 1000,
            remoteSort: true,
            sorters: [{ property: 'SEQ', direction: 'DESC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AB0021/AllMeDocd',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                    //totalProperty: 'rc'
                }
            }
        },
        TOWH: {
            model: 'WEBAPP.model.MI_WHMAST',
            pageSize: 1000,
            //autoLoad: true,
            //sorters: [{ property: 'SUPPLY_INID', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AB0012/GetTowhCombo',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                    //totalProperty: 'rc'
                }
            }
        },
        FRWH: {
            model: 'WEBAPP.model.MI_WHMAST',
            pageSize: 1000,
            autoLoad: false,
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AB0012/GetFrwh',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                    //totalProperty: 'rc'
                }
            }
        },
        USER_INFO: {
            model: 'WEBAPP.model.UserInfo',
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/Acct/Info',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                }
            }
        }
    }

});