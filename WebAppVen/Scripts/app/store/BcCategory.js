Ext.define('WEBAPP.store.BcCategory', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.BcCategory',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'XCATEGORY', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CB0012/All',
        reader: {
            type: 'json',
            rootProperty: 'etts'
        }
    }
});