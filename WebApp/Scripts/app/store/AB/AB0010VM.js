Ext.define('WEBAPP.store.AB.AB0010VM', {
    extend: 'Ext.app.ViewModel',
    //alias: 'viewmodel.AA0015',
    requires: [
        'WEBAPP.model.ME_DOCM',
        'WEBAPP.model.ME_DOCD',
        'WEBAPP.model.MI_WHMAST',
        'WEBAPP.model.UserInfo'
    ],
    stores: {
        ME_DOCM: {
            model: 'WEBAPP.model.ME_DOCM',
            pageSize: 50, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'DOCNO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AB0010/All',
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
            sorters: [{ property: 'SEQ', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AB0010/AllMeDocd',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        SUPPLY_INID: {
            model: 'WEBAPP.model.MI_WHMAST',
            pageSize: 1000,
            autoLoad: true,
            //remoteSort: true,
            //sorters: [{ property: 'SUPPLY_INID', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AB0010/GetSupplyInid',
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
                url: '/api/AB0010/GetFrwh',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                    //totalProperty: 'rc'
                }
            }
        },
        SUGGEST_QTY: {
            //model: 'WEBAPP.model.MI_WHMAST',
            //autoLoad: true,
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AB0010/GetSuggestQty',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                    //totalProperty: 'rc'
                }
            }
        },
        USER_INFO: {
            extend: 'Ext.data.Store',
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