Ext.define('WEBAPP.store.PhSmallMail', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.PhSmallMail',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'SEND_TO', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BC0010/All',
        reader: {
            type: 'json',
            rootProperty: 'etts'
        }
    }
});