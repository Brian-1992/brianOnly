Ext.define('WEBAPP.store.AA.AA0163VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.MI_WLOCINV',
        'WEBAPP.model.ME_DOCD',
        'WEBAPP.model.MI_WHMAST',
        'WEBAPP.model.MI_MATCLASS',
        'WEBAPP.model.UserInfo',
        'WEBAPP.model.ParamD',
        'WEBAPP.model.MI_MAST'
    ],
    stores: {
        AA0163: {
            remoteSort: true,
            proxy: {
                timeout: 1200000,
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AA0163/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        YM: {
            model: 'WEBAPP.model.MI_MATCLASS',
            autoLoad: true,
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AA0163/GetYmCombo',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
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