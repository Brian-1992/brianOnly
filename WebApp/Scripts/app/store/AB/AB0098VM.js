Ext.define('WEBAPP.store.AB.AB0098VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.ME_DOCM',
        'WEBAPP.model.ME_DOCD',
        'WEBAPP.model.MI_WHID',
        'WEBAPP.model.COMBO_MODEL'
    ],
    stores: {
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
                url: '/api/AB0098/All',
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
                url: '/api/AB0098/AllMeDocd',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                    //totalProperty: 'rc'
                }
            }
        },
        FRWH: {
            model: 'WEBAPP.model.MI_WHID',
            pageSize: 1000,
            autoLoad: true,
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AB0098/GetFrwhCombo',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                    //totalProperty: 'rc'
                }
            }
        },
        FLOWID: {
            pageSize: 1000,
            autoLoad: true,
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AB0098/GetFlowidCombo',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                    //totalProperty: 'rc'
                }
            }
        },
        TRANSKIND: {
            model: 'WEBAPP.model.COMBO_MODEL',
            pageSize: 1000,
            autoLoad: true,
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AB0098/GetTranskindCombo',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                    //totalProperty: 'rc'
                }
            }
        }
    }

});