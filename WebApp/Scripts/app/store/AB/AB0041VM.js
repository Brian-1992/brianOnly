Ext.define('WEBAPP.store.AB.AB0041VM', {
    extend: 'Ext.app.ViewModel',
    //alias: 'viewmodel.AA0015',
    requires: [
        'WEBAPP.model.ME_MDFM',
        'WEBAPP.model.ME_MDFD',
        'WEBAPP.model.ME_DOCM',
        'WEBAPP.model.ME_DOCD',
        'WEBAPP.model.MI_WHMAST',
        'WEBAPP.model.UserInfo'
    ],
    stores: {
        ME_MDFM: {
            model: 'WEBAPP.model.ME_MDFM',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MDFM', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AB0041/GetMaster',
                //url: '/api/AB0041/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        ME_MDFD: {
            model: 'WEBAPP.model.ME_MDFD',
            pageSize: 1000,
            remoteSort: true,
            sorters: [{ property: 'MDFM', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AB0041/GetDetail',
                //url: '/api/AA0015/AllMeDocd',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                    //totalProperty: 'rc'
                }
            }
        },
        ME_DOCM: {
            model: 'WEBAPP.model.ME_DOCM',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'DOCNO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AB0041/All',
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
                url: '/api/AA0015/AllMeDocd',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                    //totalProperty: 'rc'
                }
            }
        },
        SUPPLY_INID: {
            model: 'WEBAPP.model.MI_WHMAST',
            pageSize: 1000,
            //remoteSort: true,
            //sorters: [{ property: 'SUPPLY_INID', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AB0041/GetSupplyInid',
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
                url: '/api/AB0041/GetFrwh',
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