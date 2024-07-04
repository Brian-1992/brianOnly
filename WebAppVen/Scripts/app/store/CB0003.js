Ext.define('WEBAPP.store.CB0003', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.CB0003',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'MMCODE', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CB0003/All',
        reader: {
            type: 'json',
            rootProperty: 'etts'
        }
    },
    datachanged: function () {
        debugger
    }
});