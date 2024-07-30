Ext.define('WEBAPP.store.AB.AB0078', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AB0078',
    pageSize: 2000,
    remoteSort: true,
    sorters: [{ property: 'CONSUME_AMT', direction: 'DESC' }],
    proxy: {
        type: 'ajax',
        timeout: 120000,
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0078/All',
        reader: {
            type: 'json',
            rootProperty: 'etts'
        }
    }
});