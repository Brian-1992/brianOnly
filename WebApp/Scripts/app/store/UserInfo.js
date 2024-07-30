Ext.define('WEBAPP.store.UserInfo', {
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
});