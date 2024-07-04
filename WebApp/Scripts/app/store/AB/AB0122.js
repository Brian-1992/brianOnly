Ext.define('WEBAPP.store.AB.AB0122', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AB0122',
    pageSize: 2000,
    remoteSort: true,
    sorters: [{ property: 'CONSUME_AMT', direction: 'DESC' }],
    proxy: {
        type: 'ajax',
        timeout: 120000,
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0122/All',
        reader: {
            type: 'json',
            rootProperty: 'etts'
        }
    }
});