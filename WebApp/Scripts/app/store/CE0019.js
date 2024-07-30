Ext.define('WEBAPP.store.CE0019', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.CE0019',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'CHK_NO', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        timeout: 120000,
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CE0019/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});